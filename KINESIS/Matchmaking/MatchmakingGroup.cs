namespace KINESIS.Matchmaking;

public class MatchmakingGroup
{
    public enum GroupState
    {
        WaitingToStart,
        LoadingResources,
        InQueue,
    }

    public enum GroupUpdateType
    {
        GroupCreated = 0,
        Full = 1,
        Partial = 2,
        ParticipantAdded = 3,
        ParticipantRemoved = 4,
        ParticipantKicked = 5,
    }

    public class Participant
    {
        public readonly int AccountId;
        public readonly ClientInformation ClientInformation;
        public byte LoadingStatus;
        public readonly bool TopOfTheQueue; // TODO: move to ClientInformation?

        public Participant(int accountId, ClientInformation clientInformation, bool topOfTheQueue)
        {
            AccountId = accountId;
            ClientInformation = clientInformation;
            LoadingStatus = 0;
            TopOfTheQueue = topOfTheQueue;
        }
    }

    public Participant[] _participants = new Participant[0];
    private static int _nextGroupId = 0;

    private int _groupId = Interlocked.Increment(ref _nextGroupId);

    private byte _groupType;
    private GameFinder.TMMGameType _gameType;
    private string _gameModes;
    private string _regions;
    private byte _ranked;
    private byte _matchFidelity;
    private byte _botDifficulty;
    private byte _randomizeBots;
    private GroupState _state { get; set; } = GroupState.WaitingToStart;
    private Client.ChatChannel? _chatChannel = null;
    private readonly byte _maxGroupSize;

    public MatchmakingGroup(byte groupType, GameFinder.TMMGameType gameType, string gameModes, string regions, byte ranked, byte matchFidelity, byte botDifficulty, byte randomizeBots, byte maxGroupSize)
    {
        _groupType = groupType;
        _gameType = gameType;
        _gameModes = gameModes;
        _regions = regions;
        _ranked = ranked;
        _matchFidelity = matchFidelity;
        _botDifficulty = botDifficulty;
        _randomizeBots = randomizeBots;
        _maxGroupSize = maxGroupSize;
    }

    public bool AddParticipant(ConnectedClient connectedClient, out MatchmakingFailedToJoinReason failureReason)
    {
        while (true)
        {
            Participant[] oldParticipants = _participants;
            if (oldParticipants.Length == _maxGroupSize)
            {
                // Group is full.
                failureReason = MatchmakingFailedToJoinReason.GroupFull;
                return false;
            }
            if (oldParticipants.Any(p => p.AccountId == connectedClient.AccountId))
            {
                // There is not 'already in the group' sadly.
                failureReason = MatchmakingFailedToJoinReason.GroupFull;
                return false;
            }

            Participant[] newParticipants = new Participant[oldParticipants.Length + 1];
            Array.Copy(oldParticipants, newParticipants, oldParticipants.Length);

            // TODO: implement top-of-the-queue
            newParticipants[oldParticipants.Length] = new Participant(connectedClient.AccountId, connectedClient.ClientInformation, false);

            lock (this)
            {
                if (_state != GroupState.WaitingToStart)
                {
                    // Don't interrupt queue.
                    failureReason = MatchmakingFailedToJoinReason.AlreadyQueued;
                    return false;
                }

                if (Interlocked.CompareExchange(ref _participants, newParticipants, oldParticipants) == oldParticipants)
                {
                    // TODO: dispatch update.
                    // There is not 'success' sadly.
                    failureReason = MatchmakingFailedToJoinReason.AlreadyQueued;

                    if (newParticipants.Length == 1)
                    {
                        BroadcastUpdate(GroupUpdateType.GroupCreated, newParticipants);
                        return true;
                    }
                    else if (newParticipants.Length == 2)
                    {
                        // Create a chat channel for the group.
                        string channelName = "Group #" + _groupId;
                        string upperCaseChannelName = "GROUP #" + _groupId;
                        Client.ChatChannel newChatChannel = new Client.ChatChannel(channelName, upperCaseChannelName, "TMM Group Chat", Client.ChatChannelFlags.CannotBeJoined);
                        Client.ChatChannel newOrExistingChatChannel = ChatServer.ChatChannelsByUpperCaseName.GetOrAdd(upperCaseChannelName, newChatChannel);
                        if (ChatServer.ConnectedClientsByAccountId.TryGetValue(newParticipants[0].AccountId, out var groupLeader))
                        {
                            newOrExistingChatChannel.Add(groupLeader);
                        }
                        _chatChannel = newOrExistingChatChannel;
                    }

                    _chatChannel!.Add(connectedClient);

                    BroadcastUpdate(GroupUpdateType.Full, newParticipants);
                    return true;
                }
            }
        }
    }

    public void RemoveParticipant(ConnectedClient connectedClient)
    {
        // TODO: leave queue
        bool tryAgain = true;
        while (tryAgain)
        {
            tryAgain = false;

            Participant[] oldParticipants = _participants;
            for (int i = 0; i < oldParticipants.Length; i++)
            {
                if (oldParticipants[i].AccountId == connectedClient.AccountId)
                {
                    Participant[] newParticipants = new Participant[oldParticipants.Length - 1];
                    Array.Copy(oldParticipants, newParticipants, i);
                    Array.Copy(oldParticipants, i + 1, newParticipants, i, oldParticipants.Length - 1 - i);

                    if (Interlocked.CompareExchange(ref _participants, newParticipants, oldParticipants) == oldParticipants)
                    {
                        // is this the last participant?
                        if (oldParticipants.Length == 1)
                        {
                            if (_chatChannel != null)
                            {
                                _chatChannel.Remove(connectedClient, true);
                            }
                            return;
                        }

                        BroadcastUpdate(GroupUpdateType.ParticipantRemoved, newParticipants, connectedClient.AccountId);
                        return;
                    }
                    else
                    {
                        // CAS failed, break the inner loop and restart the outer loop.
                        tryAgain = true;
                        break;
                    }
                }
            }
        }
    }

    public void EnterQueue(IDbContextFactory<BountyContext> dbContextFactory)
    {
        Participant[] participants;
        lock (this)
        {
            if (_state == GroupState.LoadingResources || _state == GroupState.InQueue)
            {
                // Already loading resources or in the queue.
                return;
            }

            _state = GroupState.LoadingResources;
            participants = _participants;
        }

        // Broadcast an update to indicate that everyone is ready.
        BroadcastUpdate(GroupUpdateType.Partial, participants);

        // Start loading resources.
        Broadcast(new MatchmakingStartLoadingResponse(), participants);
    }

    public void LeaveQueue(bool initiatedByGameFinder)
    {
        Participant[] participants;
        lock (this)
        {
            switch (_state)
            {
                case GroupState.WaitingToStart:
                    return;
                case GroupState.LoadingResources:
                    break;
                case GroupState.InQueue:
                    // TODO: implement
                    break;
            }

            _state = GroupState.WaitingToStart;
            participants = _participants;
        }

        // Send the state change event.
        BroadcastUpdate(GroupUpdateType.Partial, participants);

        // Dispatch an event notifying all participants that the group left the queue.
        // NOTE: Order here matters. If we dispatch LeftMatchmakingQueueResponse() event first,
        // there is a bug where a loading screen is briefly visible.
        Broadcast(new LeftMatchmakingQueueResponse(), participants);
    }

    public void NotifyLoadingStatusChanged(int accountId, byte loadingStatus)
    {
        Participant[] participants = _participants;

        bool everyoneLoaded = true;
        foreach (Participant participant in participants)
        {
            if (participant.AccountId == accountId)
            {
                participant.LoadingStatus = loadingStatus;
            }
            if (participant.LoadingStatus != 100)
            {
                everyoneLoaded = false;
            }
        }

        bool enteredMatchmaking = false;
        if (everyoneLoaded)
        {
            lock (this)
            {
                if (_state == GroupState.LoadingResources)
                {
                    if (!AddToGameFinderQueue(timestampWhenJoinedQueue: Stopwatch.GetTimestamp()))
                    {
                        // Failed to join the queue, go back to WaitingToStart state.
                        _state = GroupState.WaitingToStart;
                    }
                    else
                    {
                        // TODO: implement
                        enteredMatchmaking = true;
                    }
                }
            }
        }

        BroadcastUpdate(GroupUpdateType.Partial, participants);

        if (enteredMatchmaking)
        {
            // Hides the loading interface.
            Broadcast(new EnteredMatchmakingQueueResponse(), participants);

            // Starts the clock. Optional and probably not needed since GameFinder will broadcast its own updates.
            // Broadcast(new GroupQueueStateChangedResponse(updateType: 11, averageTimeInQueueInSeconds: 42), participants);
        }
    }

    private bool AddToGameFinderQueue(long timestampWhenJoinedQueue)
    {
        // TODO: implement
        return true;
    }

    private static void Broadcast(ProtocolResponse response, Participant[] participants)
    {
        foreach (Participant participant in participants)
        {
            if (ChatServer.ConnectedClientsByAccountId.TryGetValue(participant.AccountId, out var client))
            {
                client.SendResponse(response);
            }
        }
    }

    private void BroadcastUpdate(GroupUpdateType updateType, Participant[] participants)
    {
        BroadcastUpdate(updateType, participants, 0);
    }

    private void BroadcastUpdate(GroupUpdateType updateType, Participant[] participants, int removedOrKickedAccountId)
    {
        foreach (Participant participant in participants)
        {
            byte[] friendshipStatusUnused = new byte[participants.Length];
            MatchmakingGroupUpdateResponse matchmakingGroupUpdateResponse = new MatchmakingGroupUpdateResponse(
                updateType: Convert.ToByte(updateType),
                accountId: removedOrKickedAccountId,
                groupSize: Convert.ToByte(participants.Length),
                averageTMR: Convert.ToInt16(1500),
                leaderAccountId: participants[0].AccountId,
                unknown1: 1, // 1 for ranked, 4 midwars, 5 bot, 7 riftwars? Possibly wrong.
                gameType: _gameType,
                mapName: GetMapName(_gameType),
                gameModes: _gameModes,
                regions: _regions,
                ranked: _ranked,
                matchFidelity: _matchFidelity,
                botDifficulty: _botDifficulty,
                randomizeBots: _randomizeBots,
                unknown2: "",
                playerInvitationResponses: "", // seems important.
                teamSize: _maxGroupSize,
                groupType: _groupType,
                groupParticipants: participants,
                friendshipStatus: friendshipStatusUnused,
                _state == GroupState.LoadingResources
            );

            if (ChatServer.ConnectedClientsByAccountId.TryGetValue(participant.AccountId, out var client))
            {
                client.SendResponse(matchmakingGroupUpdateResponse);
            }
        }
    }

    private static string GetMapName(GameFinder.TMMGameType gameType)
    {
        return gameType switch
        {
            GameFinder.TMMGameType.MIDWARS => "midwars",
            GameFinder.TMMGameType.RIFTWARS => "riftwars",
            GameFinder.TMMGameType.CAMPAIGN_NORMAL => "caldavar",
            GameFinder.TMMGameType.NORMAL => "caldavar",
            GameFinder.TMMGameType.CAMPAIGN_CASUAL => "caldavar_old",
            GameFinder.TMMGameType.CASUAL => "caldavar_old",
            _ => "unknown#" + gameType,
        };
    }
}
