using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Voting
{
    public sealed class MsgVoteData : NetMessage
    {
        public override MsgGroups MsgGroup => MsgGroups.Command;

        public int VoteId;
        public bool VoteActive;
        public string VoteTitle = string.Empty;
        public string VoteInitiator = string.Empty;
        public TimeSpan StartTime; // Server RealTime.
        public TimeSpan EndTime; // Server RealTime.
        public (ushort votes, string name)[] Options = default!;
        public bool IsYourVoteDirty;
        public byte[]? YourVotes; // Erida-edit
        public bool DisplayVotes;
        public int TargetEntity;
        public bool Multivariate; // Erida-edit

        public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
        {
            VoteId = buffer.ReadVariableInt32();
            VoteActive = buffer.ReadBoolean();
            buffer.ReadPadBits();

            if (!VoteActive)
                return;

            VoteTitle = buffer.ReadString();
            VoteInitiator = buffer.ReadString();
            StartTime = TimeSpan.FromTicks(buffer.ReadInt64());
            EndTime = TimeSpan.FromTicks(buffer.ReadInt64());
            DisplayVotes = buffer.ReadBoolean();
            TargetEntity = buffer.ReadVariableInt32();
            Multivariate = buffer.ReadBoolean(); // Erida-edit

            Options = new (ushort votes, string name)[buffer.ReadByte()];
            for (var i = 0; i < Options.Length; i++)
            {
                Options[i] = (buffer.ReadUInt16(), buffer.ReadString());
            }

            IsYourVoteDirty = buffer.ReadBoolean();
            if (IsYourVoteDirty)
            {
            // Erida-start
                if (buffer.ReadBoolean())
                {
                    var voteCount = buffer.ReadByte();
                    YourVotes = new byte[voteCount];
                    for (var i = 0; i < voteCount; i++)
                    {
                        YourVotes[i] = buffer.ReadByte();
                    }
                }
                else
                {
                    YourVotes = null;
                }
            }
            // Erida-end
        }

        public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
        {
            buffer.WriteVariableInt32(VoteId);
            buffer.Write(VoteActive);
            buffer.WritePadBits();

            if (!VoteActive)
                return;

            buffer.Write(VoteTitle);
            buffer.Write(VoteInitiator);
            buffer.Write(StartTime.Ticks);
            buffer.Write(EndTime.Ticks);
            buffer.Write(DisplayVotes);
            buffer.WriteVariableInt32(TargetEntity);
            buffer.Write(Multivariate); // Erida-edit

            buffer.Write((byte) Options.Length);
            foreach (var (votes, name) in Options)
            {
                buffer.Write(votes);
                buffer.Write(name);
            }

            buffer.Write(IsYourVoteDirty);
            if (IsYourVoteDirty)
            {
                // Erida-start
                buffer.Write(YourVotes != null);
                if (YourVotes != null)
                {
                    buffer.Write((byte) YourVotes.Length);
                    foreach (var vote in YourVotes)
                    {
                        buffer.Write(vote);
                    }
                }
            }
            // Erida-end
        }

        public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
