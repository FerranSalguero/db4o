/* This file is part of the db4o object database http://www.db4o.com

Copyright (C) 2004 - 2011  Versant Corporation http://www.versant.com

db4o is free software; you can redistribute it and/or modify it under
the terms of version 3 of the GNU General Public License as published
by the Free Software Foundation.

db4o is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or
FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
for more details.

You should have received a copy of the GNU General Public License along
with this program.  If not, see http://www.gnu.org/licenses/. */

using System.IO;
using System.Text;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Encoding;
using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Internal.Slots;
using Sharpen;
using Sharpen.IO;

namespace Db4objects.Db4o
{
    /// <exclude></exclude>
    public class DTrace
    {
        public static bool enabled = false;
        public static bool writeToLogFile = false;
        public static bool writeToConsole = true;
        private static readonly string logFilePath = "C://";
        private static string logFileName;
        private static readonly object Lock = new object();
        private static readonly LatinStringIO stringIO = new LatinStringIO();
        public static RandomAccessFile _logFile;
        private static readonly int Unused = -1;
        private static long[] _rangeStart;
        private static long[] _rangeEnd;
        private static int _rangeCount;
        public static long _eventNr;
        private static long[] _breakEventNrs;
        private static int _breakEventCount;
        private static bool _breakAfterEvent;
        private static bool _trackEventsWithoutRange;
        public static DTrace AddToClassIndex;
        public static DTrace BeginTopLevelCall;
        public static DTrace Bind;
        public static DTrace BlockingQueueStoppedException;
        public static DTrace BtreeNodeCommitOrRollback;
        public static DTrace BtreeNodeRemove;
        public static DTrace BtreeProduceNode;
        public static DTrace CandidateRead;
        public static DTrace ClassmetadataById;
        public static DTrace ClassmetadataInit;
        public static DTrace ClientMessageLoopException;
        public static DTrace Close;
        public static DTrace CloseCalled;
        public static DTrace CollectChildren;
        public static DTrace Commit;
        public static DTrace Continueset;
        public static DTrace CreateCandidate;
        public static DTrace Delete;
        public static DTrace Donotinclude;
        public static DTrace EndTopLevelCall;
        public static DTrace EvaluateSelf;
        public static DTrace FatalException;
        public static DTrace FileFree;
        public static DTrace FileRead;
        public static DTrace FileWrite;
        public static DTrace Free;
        public static DTrace FreespacemanagerGetSlot;
        public static DTrace FreespacemanagerRamFree;
        public static DTrace FreespacemanagerBtreeFree;
        public static DTrace FreeOnCommit;
        public static DTrace FreeOnRollback;
        public static DTrace FreePointerOnRollback;
        public static DTrace GetSlot;
        public static DTrace GetPointerSlot;
        public static DTrace GetFreespaceRam;
        public static DTrace GetYapobject;
        public static DTrace IdTreeAdd;
        public static DTrace IdTreeRemove;
        public static DTrace IoCopy;
        public static DTrace JustSet;
        public static DTrace NewInstance;
        public static DTrace NotifySlotCreated;
        public static DTrace NotifySlotUpdated;
        public static DTrace NotifySlotDeleted;
        public static DTrace ObjectReferenceCreated;
        public static DTrace PersistentBaseNewSlot;
        public static DTrace PersistentOwnLength;
        public static DTrace PersistentbaseSetId;
        public static DTrace PersistentbaseWrite;
        public static DTrace ProduceSlotChange;
        public static DTrace QueryProcess;
        public static DTrace ReadArrayWrapper;
        public static DTrace ReadBytes;
        public static DTrace ReadSlot;
        public static DTrace ReferenceRemoved;
        public static DTrace RegularSeek;
        public static DTrace RemoveFromClassIndex;
        public static DTrace RereadOldUuid;
        public static DTrace ServerMessageLoopException;
        public static DTrace SlotMapped;
        public static DTrace SlotCommitted;
        public static DTrace SlotFreeOnCommit;
        public static DTrace SlotFreeOnRollbackId;
        public static DTrace SlotFreeOnRollbackAddress;
        public static DTrace SlotRead;
        public static DTrace TransCommit;
        public static DTrace TransDontDelete;
        public static DTrace TransDelete;
        public static DTrace TransFlush;
        public static DTrace WriteBytes;
        public static DTrace WritePointer;
        public static DTrace WriteXbytes;
        public static DTrace WriteUpdateAdjustIndexes;
        private static DTrace[] all;
        private static int current;
        private readonly string _tag;
        private bool _break;
        private bool _enabled;
        private bool _log;

        static DTrace()
        {
            Init();
        }

        private DTrace(bool enabled_, bool break_, string tag_, bool log_)
        {
            if (enabled)
            {
                _enabled = enabled_;
                _break = break_;
                _tag = tag_;
                _log = log_;
                if (all == null)
                {
                    all = new DTrace[100];
                }
                all[current++] = this;
            }
        }

        private static void BreakPoint()
        {
            if (enabled)
            {
                var xxx = 1;
            }
        }

        private static void Configure()
        {
            if (enabled)
            {
            }
        }

        // addRange(15);
        // breakOnEvent(540);
        //        	
        //        	addRangeWithEnd(448, 460);
        //        	addRangeWithLength(770,53);
        // breakOnEvent(125);
        //            trackEventsWithoutRange();
        //            turnAllOffExceptFor(new DTrace[] {WRITE_BYTES});
        //            turnAllOffExceptFor(new DTrace[] {
        //                PERSISTENT_OWN_LENGTH,
        //                });
        //            turnAllOffExceptFor(new DTrace[] {
        //                GET_SLOT,
        //                FILE_FREE,
        //                TRANS_COMMIT,
        //                });
        // turnAllOffExceptFor(new DTrace[] {WRITE_BYTES});
        //            turnAllOffExceptFor(new DTrace[] {BTREE_NODE_REMOVE, BTREE_NODE_COMMIT_OR_ROLLBACK YAPMETA_SET_ID});
        private static void Init()
        {
            if (enabled)
            {
                AddToClassIndex = new DTrace(true, true, "add to class index tree"
                    , true);
                BeginTopLevelCall = new DTrace(true, true, "begin top level call"
                    , true);
                Bind = new DTrace(true, true, "bind", true);
                BlockingQueueStoppedException = new DTrace(true, true, "blocking queue stopped exception"
                    , true);
                BtreeNodeRemove = new DTrace(true, true, "btreenode remove", true
                    );
                BtreeNodeCommitOrRollback = new DTrace(true, true, "btreenode commit or rollback"
                    , true);
                BtreeProduceNode = new DTrace(true, true, "btree produce node", true
                    );
                CandidateRead = new DTrace(true, true, "candidate read", true);
                ClassmetadataById = new DTrace(true, true, "classmetadata by id",
                    true);
                ClassmetadataInit = new DTrace(true, true, "classmetadata init",
                    true);
                ClientMessageLoopException = new DTrace(true, true, "client message loop exception"
                    , true);
                Close = new DTrace(true, true, "close", true);
                CloseCalled = new DTrace(true, true, "close called", true);
                CollectChildren = new DTrace(true, true, "collect children", true
                    );
                Commit = new DTrace(false, false, "commit", true);
                Continueset = new DTrace(true, true, "continueset", true);
                CreateCandidate = new DTrace(true, true, "create candidate", true
                    );
                Delete = new DTrace(true, true, "delete", true);
                Donotinclude = new DTrace(true, true, "donotinclude", true);
                EndTopLevelCall = new DTrace(true, true, "end top level call", true
                    );
                EvaluateSelf = new DTrace(true, true, "evaluate self", true);
                FatalException = new DTrace(true, true, "fatal exception", true);
                Free = new DTrace(true, true, "free", true);
                FileFree = new DTrace(true, true, "fileFree", true);
                FileRead = new DTrace(true, true, "fileRead", true);
                FileWrite = new DTrace(true, true, "fileWrite", true);
                FreespacemanagerGetSlot = new DTrace(true, true, "FreespaceManager getSlot"
                    , true);
                FreespacemanagerRamFree = new DTrace(true, true, "InMemoryfreespaceManager free"
                    , true);
                FreespacemanagerBtreeFree = new DTrace(true, true, "BTreeFreeSpaceManager free"
                    , true);
                FreeOnCommit = new DTrace(true, true, "trans freeOnCommit", true);
                FreeOnRollback = new DTrace(true, true, "trans freeOnRollback", true
                    );
                FreePointerOnRollback = new DTrace(true, true, "freePointerOnRollback"
                    , true);
                GetPointerSlot = new DTrace(true, true, "getPointerSlot", true);
                GetSlot = new DTrace(true, true, "getSlot", true);
                GetFreespaceRam = new DTrace(true, true, "getFreespaceRam", true);
                GetYapobject = new DTrace(true, true, "get ObjectReference", true
                    );
                IdTreeAdd = new DTrace(true, true, "id tree add", true);
                IdTreeRemove = new DTrace(true, true, "id tree remove", true);
                IoCopy = new DTrace(true, true, "io copy", true);
                JustSet = new DTrace(true, true, "just set", true);
                NewInstance = new DTrace(true, true, "newInstance", true);
                NotifySlotCreated = new DTrace(true, true, "notifySlotCreated", true
                    );
                NotifySlotUpdated = new DTrace(true, true, "notify Slot updated",
                    true);
                NotifySlotDeleted = new DTrace(true, true, "notifySlotDeleted", true
                    );
                ObjectReferenceCreated = new DTrace(true, true, "new ObjectReference"
                    , true);
                PersistentBaseNewSlot = new DTrace(true, true, "PersistentBase new slot"
                    , true);
                PersistentOwnLength = new DTrace(true, true, "Persistent own length"
                    , true);
                PersistentbaseWrite = new DTrace(true, true, "persistentbase write"
                    , true);
                PersistentbaseSetId = new DTrace(true, true, "persistentbase setid"
                    , true);
                ProduceSlotChange = new DTrace(true, true, "produce slot change",
                    true);
                QueryProcess = new DTrace(true, true, "query process", true);
                ReadArrayWrapper = new DTrace(true, true, "read array wrapper", true
                    );
                ReadBytes = new DTrace(true, true, "readBytes", true);
                ReadSlot = new DTrace(true, true, "read slot", true);
                ReferenceRemoved = new DTrace(true, true, "reference removed", true
                    );
                RegularSeek = new DTrace(true, true, "regular seek", true);
                RemoveFromClassIndex = new DTrace(true, true, "trans removeFromClassIndexTree"
                    , true);
                RereadOldUuid = new DTrace(true, true, "reread old uuid", true);
                ServerMessageLoopException = new DTrace(true, true, "server message loop exception"
                    , true);
                SlotMapped = new DTrace(true, true, "slot mapped", true);
                SlotCommitted = new DTrace(true, true, "slot committed", true);
                SlotFreeOnCommit = new DTrace(true, true, "slot free on commit",
                    true);
                SlotFreeOnRollbackId = new DTrace(true, true, "slot free on rollback id"
                    , true);
                SlotFreeOnRollbackAddress = new DTrace(true, true, "slot free on rollback address"
                    , true);
                SlotRead = new DTrace(true, true, "slot read", true);
                TransCommit = new DTrace(true, true, "trans commit", true);
                TransDelete = new DTrace(true, true, "trans delete", true);
                TransDontDelete = new DTrace(true, true, "trans dontDelete", true
                    );
                TransFlush = new DTrace(true, true, "trans flush", true);
                WriteBytes = new DTrace(true, true, "writeBytes", true);
                WritePointer = new DTrace(true, true, "write pointer", true);
                WriteUpdateAdjustIndexes = new DTrace(true, true, "trans writeUpdateDeleteMembers"
                    , true);
                WriteXbytes = new DTrace(true, true, "writeXBytes", true);
                Configure();
            }
        }

        private static void TrackEventsWithoutRange()
        {
            _trackEventsWithoutRange = true;
        }

        public virtual void Log()
        {
            if (enabled)
            {
                Log(Unused);
            }
        }

        public virtual void Log(string msg)
        {
            if (enabled)
            {
                Log(Unused, msg);
            }
        }

        public virtual void Log(long p)
        {
            if (enabled)
            {
                LogLength(p, 1);
            }
        }

        public virtual void LogInfo(string info)
        {
            if (enabled)
            {
                LogEnd(Unused, Unused, 0, info);
            }
        }

        public virtual void Log(long p, string info)
        {
            if (enabled)
            {
                LogEnd(Unused, p, 0, info);
            }
        }

        public virtual void LogLength(long start, long length)
        {
            if (enabled)
            {
                LogLength(Unused, start, length);
            }
        }

        public virtual void LogLength(long id, long start, long length)
        {
            if (enabled)
            {
                LogEnd(id, start, start + length - 1);
            }
        }

        public virtual void LogLength(Slot slot)
        {
            if (enabled)
            {
                LogLength(Unused, slot);
            }
        }

        public virtual void LogLength(long id, Slot slot)
        {
            if (enabled)
            {
                if (slot == null)
                {
                    return;
                }
                LogLength(id, slot.Address(), slot.Length());
            }
        }

        public virtual void LogEnd(long start, long end)
        {
            if (enabled)
            {
                LogEnd(Unused, start, end);
            }
        }

        public virtual void LogEnd(long id, long start, long end)
        {
            if (enabled)
            {
                LogEnd(id, start, end, null);
            }
        }

        public virtual void LogEnd(long id, long start, long end, string info)
        {
            //    	if(! Deploy.log){
            //    		return;
            //    	}
            if (enabled)
            {
                if (!_enabled)
                {
                    return;
                }
                var inRange = false;
                if (_rangeCount == 0)
                {
                    inRange = true;
                }
                for (var i = 0; i < _rangeCount; i++)
                {
                    // Case 0 ID in range
                    if (id >= _rangeStart[i] && id <= _rangeEnd[i])
                    {
                        inRange = true;
                        break;
                    }
                    // Case 1 start in range
                    if (start >= _rangeStart[i] && start <= _rangeEnd[i])
                    {
                        inRange = true;
                        break;
                    }
                    if (end != 0)
                    {
                        // Case 2 end in range
                        if (end >= _rangeStart[i] && end <= _rangeEnd[i])
                        {
                            inRange = true;
                            break;
                        }
                        // Case 3 start before range, end after range
                        if (start <= _rangeStart[i] && end >= _rangeEnd[i])
                        {
                            inRange = true;
                            break;
                        }
                    }
                }
                if (inRange || (_trackEventsWithoutRange && (start == Unused)))
                {
                    if (_log)
                    {
                        _eventNr++;
                        var sb = new StringBuilder(":");
                        sb.Append(FormatInt(_eventNr, 6));
                        sb.Append(":");
                        sb.Append(FormatInt(id));
                        sb.Append(":");
                        sb.Append(FormatInt(start));
                        sb.Append(":");
                        if (end != 0 && start != end)
                        {
                            sb.Append(FormatInt(end));
                            sb.Append(":");
                            sb.Append(FormatInt(end - start + 1));
                        }
                        else
                        {
                            sb.Append(FormatUnused());
                            sb.Append(":");
                            sb.Append(FormatUnused());
                        }
                        sb.Append(":");
                        if (info != null)
                        {
                            sb.Append(" " + info + " ");
                            sb.Append(":");
                        }
                        sb.Append(" ");
                        sb.Append(_tag);
                        LogToOutput(sb.ToString());
                    }
                    if (_break)
                    {
                        if (_breakEventCount > 0)
                        {
                            for (var i = 0; i < _breakEventCount; i++)
                            {
                                if (_breakEventNrs[i] == _eventNr)
                                {
                                    BreakPoint();
                                    break;
                                }
                            }
                            if (_breakAfterEvent)
                            {
                                for (var i = 0; i < _breakEventCount; i++)
                                {
                                    if (_breakEventNrs[i] <= _eventNr)
                                    {
                                        BreakPoint();
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            BreakPoint();
                        }
                    }
                }
            }
        }

        private string FormatUnused()
        {
            return FormatInt(Unused);
        }

        private static void LogToOutput(string msg)
        {
            if (enabled)
            {
                LogToFile(msg);
                LogToConsole(msg);
            }
        }

        private static void LogToConsole(string msg)
        {
            if (enabled)
            {
                if (writeToConsole)
                {
                    Runtime.Out.WriteLine(msg);
                }
            }
        }

        private static void LogToFile(string msg)
        {
            if (enabled)
            {
                if (!writeToLogFile)
                {
                    return;
                }
                lock (Lock)
                {
                    if (_logFile == null)
                    {
                        try
                        {
                            _logFile = new RandomAccessFile(LogFile(), "rw");
                            LogToFile("\r\n\r\n ********** BEGIN LOG ********** \r\n\r\n ");
                        }
                        catch (IOException e)
                        {
                            Runtime.PrintStackTrace(e);
                        }
                    }
                    msg = DateHandlerBase.Now() + "\r\n" + msg + "\r\n";
                    var bytes = stringIO.Write(msg);
                    try
                    {
                        _logFile.Write(bytes);
                    }
                    catch (IOException e)
                    {
                        Runtime.PrintStackTrace(e);
                    }
                }
            }
        }

        private static string LogFile()
        {
            if (enabled)
            {
                if (logFileName != null)
                {
                    return logFileName;
                }
                logFileName = "db4oDTrace_" + DateHandlerBase.Now() + "_" + SignatureGenerator.GenerateSignature
                    () + ".log";
                logFileName = logFileName.Replace(' ', '_');
                logFileName = logFileName.Replace(':', '_');
                logFileName = logFileName.Replace('-', '_');
                return logFilePath + logFileName;
            }
            return null;
        }

        public static void AddRange(long pos)
        {
            if (enabled)
            {
                AddRangeWithEnd(pos, pos);
            }
        }

        public static void AddRangeWithLength(long start, long length)
        {
            if (enabled)
            {
                AddRangeWithEnd(start, start + length - 1);
            }
        }

        public static void AddRangeWithEnd(long start, long end)
        {
            if (enabled)
            {
                if (_rangeStart == null)
                {
                    _rangeStart = new long[1000];
                    _rangeEnd = new long[1000];
                }
                _rangeStart[_rangeCount] = start;
                _rangeEnd[_rangeCount] = end;
                _rangeCount++;
            }
        }

        //    private static void breakFromEvent(long eventNr){
        //        breakOnEvent(eventNr);
        //        _breakAfterEvent = true;
        //    }
        private static void BreakOnEvent(long eventNr)
        {
            if (enabled)
            {
                if (_breakEventNrs == null)
                {
                    _breakEventNrs = new long[100];
                }
                _breakEventNrs[_breakEventCount] = eventNr;
                _breakEventCount++;
            }
        }

        private string FormatInt(long i, int len)
        {
            if (enabled)
            {
                var str = "              ";
                if (i != Unused)
                {
                    str += i + " ";
                }
                return Runtime.Substring(str, str.Length - len);
            }
            return null;
        }

        private string FormatInt(long i)
        {
            if (enabled)
            {
                return FormatInt(i, 10);
            }
            return null;
        }

        private static void TurnAllOffExceptFor(DTrace[] these)
        {
            if (enabled)
            {
                for (var i = 0; i < all.Length; i++)
                {
                    if (all[i] == null)
                    {
                        break;
                    }
                    var turnOff = true;
                    for (var j = 0; j < these.Length; j++)
                    {
                        if (all[i] == these[j])
                        {
                            turnOff = false;
                            break;
                        }
                    }
                    if (turnOff)
                    {
                        all[i]._break = false;
                        all[i]._enabled = false;
                        all[i]._log = false;
                    }
                }
            }
        }

        public static void NoWarnings()
        {
            BreakOnEvent(0);
            TrackEventsWithoutRange();
        }
    }
}