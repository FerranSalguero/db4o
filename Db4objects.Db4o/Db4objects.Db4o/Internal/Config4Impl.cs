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

using System;
using System.Collections;
using System.IO;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Config.Encoding;
using Db4objects.Db4o.Diagnostic;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal.Activation;
using Db4objects.Db4o.Internal.Config;
using Db4objects.Db4o.Internal.Diagnostic;
using Db4objects.Db4o.Internal.Encoding;
using Db4objects.Db4o.Internal.Freespace;
using Db4objects.Db4o.Internal.Handlers;
using Db4objects.Db4o.Internal.Ids;
using Db4objects.Db4o.Internal.References;
using Db4objects.Db4o.IO;
using Db4objects.Db4o.Messaging;
using Db4objects.Db4o.Reflect;
using Db4objects.Db4o.Reflect.Core;
using Db4objects.Db4o.Reflect.Generic;
using Db4objects.Db4o.Typehandlers;
using Sharpen;
using File = Sharpen.IO.File;

namespace Db4objects.Db4o.Internal
{
    /// <summary>Configuration template for creating new db4o files</summary>
    /// <exclude></exclude>
    public sealed partial class Config4Impl : IConfiguration, IDeepClone, IMessageSender
        , IFreespaceConfiguration, IQueryConfiguration, IClientServerConfiguration
    {
        public const int PrefetchSlotCacheSizeFactor = 10;
        private const int MaximumPrefetchSlotCacheSize = 10000;
        private static readonly KeySpec ActivationDepthKey = new KeySpec(5);

        private static readonly KeySpec ActivationDepthProviderKey = new KeySpec(LegacyActivationDepthProvider
            .Instance);

        private static readonly KeySpec UpdateDepthProviderKey = new KeySpec(new LegacyUpdateDepthProvider
            ());

        private static readonly KeySpec AllowVersionUpdatesKey = new KeySpec(false);
        private static readonly KeySpec AsynchronousSyncKey = new KeySpec(false);
        private static readonly KeySpec AutomaticShutdownKey = new KeySpec(true);
        private static readonly KeySpec BlocksizeKey = new KeySpec((byte) 1);
        private static readonly KeySpec BlobPathKey = new KeySpec(null);
        private static readonly KeySpec BtreeNodeSizeKey = new KeySpec(201);
        private static readonly KeySpec CallbacksKey = new KeySpec(CallBackMode.All);

        private static readonly KeySpec CallConstructorsKey = new KeySpec(TernaryBool.Unspecified
            );

        private static readonly KeySpec ConfigurationItemsKey = new KeySpec(null);
        private static readonly KeySpec ConfiguredReflectorKey = new KeySpec(null);

        private static readonly KeySpec ClassActivationDepthConfigurableKey = new KeySpec
            (true);

        private static readonly KeySpec ClassloaderKey = new KeySpec(null);

        private static readonly KeySpec ClientServerFactoryKey = new KeySpec(new _IDeferred_75
            ());

        private static readonly KeySpec DatabaseGrowthSizeKey = new KeySpec(0);
        private static readonly KeySpec DetectSchemaChangesKey = new KeySpec(true);
        private static readonly KeySpec DiagnosticKey = new KeySpec(new _IDeferred_85());
        private static readonly KeySpec DisableCommitRecoveryKey = new KeySpec(false);
        private static readonly KeySpec DiscardFreespaceKey = new KeySpec(0);

        private static readonly IStringEncoding DefaultStringEncoding = StringEncodings.Unicode
            ();

        private static readonly KeySpec StringEncodingKey = new KeySpec(DefaultStringEncoding
            );

        private static readonly KeySpec EncodingKey = new KeySpec(BuiltInStringEncoding.EncodingByteForEncoding
            (DefaultStringEncoding));

        private static readonly KeySpec EncryptKey = new KeySpec(false);

        private static readonly KeySpec EnvironmentContributionsKey = new KeySpec(new _IDeferred_103
            ());

        private static readonly KeySpec ExceptionalClassesKey = new KeySpec(null);
        private static readonly KeySpec ExceptionsOnNotStorableKey = new KeySpec(true);
        private static readonly KeySpec FileBasedTransactionLogKey = new KeySpec(false);
        private static readonly KeySpec FreespaceFillerKey = new KeySpec(null);

        private static readonly KeySpec FreespaceSystemKey = new KeySpec(AbstractFreespaceManager
            .FmDefault);

        private static readonly KeySpec GenerateUuidsKey = new KeySpec(ConfigScope.Individually
            );

        private static readonly KeySpec GenerateCommitTimestampsKey = new KeySpec(TernaryBool
            .Unspecified);

        private static readonly KeySpec IdSystemKey = new KeySpec(StandardIdSystemFactory
            .Default);

        private static readonly KeySpec IdSystemCustomFactoryKey = new KeySpec(null);

        private static readonly KeySpec QueryEvaluationModeKey = new KeySpec(QueryEvaluationMode
            .Immediate);

        private static readonly KeySpec LockFileKey = new KeySpec(true);
        private static readonly KeySpec MessageRecipientKey = new KeySpec(null);
        private static readonly KeySpec OptimizeNqKey = new KeySpec(true);
        private static readonly KeySpec OutstreamKey = new KeySpec(null);
        private static readonly KeySpec PasswordKey = new KeySpec(null);

        private static readonly KeySpec ClientQueryResultIteratorFactoryKey = new KeySpec
            (null);

        private static readonly KeySpec PrefetchIdCountKey = new KeySpec(10);
        private static readonly KeySpec PrefetchObjectCountKey = new KeySpec(10);
        private static readonly KeySpec PrefetchDepthKey = new KeySpec(0);
        private static readonly KeySpec PrefetchSlotCacheSizeKey = new KeySpec(0);
        private static readonly KeySpec ReadAsKey = new KeySpec(new _IDeferred_155());
        private static readonly KeySpec RecoveryModeKey = new KeySpec(false);
        private static readonly KeySpec ReflectorKey = new KeySpec(null);
        private static readonly KeySpec RenameKey = new KeySpec(null);
        private static readonly KeySpec ReservedStorageSpaceKey = new KeySpec(0);
        private static readonly KeySpec SingleThreadedClientKey = new KeySpec(false);
        private static readonly KeySpec TestConstructorsKey = new KeySpec(true);

        private static readonly KeySpec TimeoutClientSocketKey = new KeySpec(Const4.ClientSocketTimeout
            );

        private static readonly KeySpec TimeoutServerSocketKey = new KeySpec(Const4.ServerSocketTimeout
            );

        private static readonly KeySpec UpdateDepthKey = new KeySpec(1);

        private static readonly KeySpec WeakReferenceCollectionIntervalKey = new KeySpec(
            1000);

        private static readonly KeySpec WeakReferencesKey = new KeySpec(true);

        private static readonly KeySpec StorageFactoryKey = new KeySpec(new CachingStorage
            (new FileStorage()));

        private static readonly KeySpec AliasesKey = new KeySpec(null);
        private static readonly KeySpec BatchMessagesKey = new KeySpec(true);
        private static readonly KeySpec MaxBatchQueueSizeKey = new KeySpec(int.MaxValue);
        private static readonly KeySpec TaintedKey = new KeySpec(false);

        private static readonly KeySpec ReferenceSystemFactoryKey = new KeySpec(new _IReferenceSystemFactory_193
            ());

        private static readonly KeySpec NameProviderKey = new KeySpec(new _INameProvider_199
            ());

        private static readonly KeySpec MaxStackDepthKey = new KeySpec("Dalvik".Equals(Runtime
            .GetProperty("java.vm.name"))
            ? 2
            : Const4.DefaultMaxStackDepth);

        private KeySpecHashtable4 _config = new KeySpecHashtable4(50);
        private ObjectContainerBase _container;
        private bool _internStrings;
        private int _messageLevel;
        private EventHandler<EventArgs> _prefetchSettingsChanged;
        private bool _prefetchSlotCacheSizeModifiedExternally;
        private bool _readOnly;
        private Collection4 _registeredTypeHandlers;

        public IMessageSender GetMessageSender()
        {
            return this;
        }

        public void SetMessageRecipient(IMessageRecipient messageRecipient)
        {
            _config.Put(MessageRecipientKey, messageRecipient);
        }

        public void SingleThreadedClient(bool flag)
        {
            _config.Put(SingleThreadedClientKey, flag);
        }

        public void TimeoutClientSocket(int milliseconds)
        {
            _config.Put(TimeoutClientSocketKey, milliseconds);
        }

        public void TimeoutServerSocket(int milliseconds)
        {
            _config.Put(TimeoutServerSocketKey, milliseconds);
        }

        public void PrefetchIDCount(int prefetchIDCount)
        {
            _config.Put(PrefetchIdCountKey, prefetchIDCount);
        }

        public void PrefetchObjectCount(int prefetchObjectCount)
        {
            _config.Put(PrefetchObjectCountKey, prefetchObjectCount);
            EnsurePrefetchSlotCacheSize();
        }

        public void BatchMessages(bool flag)
        {
            _config.Put(BatchMessagesKey, flag);
        }

        public void MaxBatchQueueSize(int maxSize)
        {
            _config.Put(MaxBatchQueueSizeKey, maxSize);
        }

        public void PrefetchDepth(int prefetchDepth)
        {
            _config.Put(PrefetchDepthKey, prefetchDepth);
            EnsurePrefetchSlotCacheSize();
        }

        public void PrefetchSlotCacheSize(int slotCacheSize)
        {
            _prefetchSlotCacheSizeModifiedExternally = true;
            _config.Put(PrefetchSlotCacheSizeKey, slotCacheSize);
            if (null != _prefetchSettingsChanged)
                _prefetchSettingsChanged(null, EventArgs.Empty
                    );
        }

        // TODO find a better place to do this, and use AndroidConfiguration instead.
        //  is null in the global configuration until deepClone is called
        // The following are very frequently being asked for, so they show up in the profiler. 
        // Let's keep them out of the Hashtable.
        public int ActivationDepth()
        {
            return _config.GetAsInt(ActivationDepthKey);
        }

        // FIXME: circular cs dependancy. Improve.
        public void ActivationDepth(int depth)
        {
            _config.Put(ActivationDepthKey, depth);
        }

        public void Add(IConfigurationItem item)
        {
            item.Prepare(this);
            SafeConfigurationItems().Put(item, item);
        }

        public void AllowVersionUpdates(bool flag)
        {
            _config.Put(AllowVersionUpdatesKey, flag);
        }

        public void AutomaticShutDown(bool flag)
        {
            _config.Put(AutomaticShutdownKey, flag);
        }

        public void BlockSize(int bytes)
        {
            if (bytes < 1 || bytes > 127)
            {
                throw new ArgumentException();
            }
            GlobalSettingOnly();
            _config.Put(BlocksizeKey, (byte) bytes);
        }

        public void BTreeNodeSize(int size)
        {
            _config.Put(BtreeNodeSizeKey, size);
        }

        public void BTreeCacheHeight(int height)
        {
        }

        public void Callbacks(bool turnOn)
        {
            CallbackMode(turnOn ? CallBackMode.All : CallBackMode.None);
        }

        public void CallConstructors(bool flag)
        {
            _config.Put(CallConstructorsKey, TernaryBool.ForBoolean(flag));
        }

        public void ClassActivationDepthConfigurable(bool turnOn)
        {
            _config.Put(ClassActivationDepthConfigurableKey, turnOn);
        }

        public void DatabaseGrowthSize(int bytes)
        {
            _config.Put(DatabaseGrowthSizeKey, bytes);
        }

        public void DetectSchemaChanges(bool flag)
        {
            _config.Put(DetectSchemaChangesKey, flag);
        }

        public void DisableCommitRecovery()
        {
            _config.Put(DisableCommitRecoveryKey, true);
        }

        [Obsolete]
        public void Encrypt(bool flag)
        {
            GlobalSettingOnly();
            _config.Put(EncryptKey, flag);
        }

        public void ExceptionsOnNotStorable(bool flag)
        {
            _config.Put(ExceptionsOnNotStorableKey, flag);
        }

        public IFreespaceConfiguration Freespace()
        {
            return this;
        }

        public void GenerateUUIDs(ConfigScope scope)
        {
            _config.Put(GenerateUuidsKey, scope);
        }

        public void GenerateVersionNumbers(ConfigScope scope)
        {
            if (scope == ConfigScope.Individually)
            {
                throw new NotSupportedException();
            }
            GenerateCommitTimestamps(scope == ConfigScope.Globally);
        }

        public void GenerateCommitTimestamps(bool flag)
        {
            _config.Put(GenerateCommitTimestampsKey, TernaryBool.ForBoolean(flag));
        }

        public void InternStrings(bool doIntern)
        {
            _internStrings = doIntern;
        }

        public void Io(IoAdapter adapter)
        {
            GlobalSettingOnly();
            Storage = new IoAdapterStorage(adapter);
        }

        public void LockDatabaseFile(bool flag)
        {
            _config.Put(LockFileKey, flag);
        }

        public void MarkTransient(string marker)
        {
            Platform4.MarkTransient(marker);
        }

        public void MessageLevel(int level)
        {
            _messageLevel = level;
            if (OutStream() == null)
            {
                SetOut(Runtime.Out);
            }
        }

        public void OptimizeNativeQueries(bool optimizeNQ)
        {
            _config.Put(OptimizeNqKey, optimizeNQ);
        }

        public bool OptimizeNativeQueries()
        {
            return _config.GetAsBoolean(OptimizeNqKey);
        }

        public IObjectClass ObjectClass(object clazz)
        {
            string className = null;
            if (clazz is string)
            {
                className = (string) clazz;
            }
            else
            {
                var claxx = ReflectorFor(clazz);
                if (claxx == null)
                {
                    return null;
                }
                className = claxx.GetName();
            }
            if (ReflectPlatform.FullyQualifiedName(typeof (object)).Equals(className))
            {
                throw new ArgumentException("Configuration of the Object class is not supported."
                    );
            }
            var xClasses = ExceptionalClasses();
            var c4c = (Config4Class) xClasses.Get(className);
            if (c4c == null)
            {
                c4c = new Config4Class(this, className);
                xClasses.Put(className, c4c);
            }
            return c4c;
        }

        [Obsolete]
        public void Password(string pw)
        {
            GlobalSettingOnly();
            _config.Put(PasswordKey, pw);
        }

        public void ReadOnly(bool flag)
        {
            _readOnly = flag;
        }

        public void ReflectWith(IReflector reflect)
        {
            if (_container != null)
            {
                Exceptions4.ThrowRuntimeException(46);
            }
            // see readable message for code in Messages.java
            if (reflect == null)
            {
                throw new ArgumentNullException();
            }
            _config.Put(ConfiguredReflectorKey, reflect);
            _config.Put(ReflectorKey, null);
        }

        /// <exception cref="Db4objects.Db4o.Ext.DatabaseReadOnlyException"></exception>
        public void ReserveStorageSpace(long byteCount)
        {
            var reservedStorageSpace = (int) byteCount;
            if (reservedStorageSpace < 0)
            {
                reservedStorageSpace = 0;
            }
            _config.Put(ReservedStorageSpaceKey, reservedStorageSpace);
            if (_container != null)
            {
                _container.Reserve(reservedStorageSpace);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void SetBlobPath(string path)
        {
            EnsureDirExists(path);
            _config.Put(BlobPathKey, path);
        }

        [Obsolete]
        public void SetOut(TextWriter outStream)
        {
            _config.Put(OutstreamKey, outStream);
            if (_container != null)
            {
                _container.LogMsg(19, Db4oFactory.Version());
            }
            else
            {
                Messages.LogMsg(this, 19, Db4oFactory.Version());
            }
        }

        public void StringEncoding(IStringEncoding encoding)
        {
            _config.Put(StringEncodingKey, encoding);
            _config.Put(EncodingKey, BuiltInStringEncoding.EncodingByteForEncoding(encoding));
        }

        public void TestConstructors(bool flag)
        {
            _config.Put(TestConstructorsKey, flag);
        }

        public void UpdateDepth(int depth)
        {
            if (depth < 0)
            {
                throw new ArgumentException("update depth must not be negative");
            }
            var dp = DiagnosticProcessor(
                );
            if (dp.Enabled())
            {
                dp.CheckUpdateDepth(depth);
            }
            _config.Put(UpdateDepthKey, depth);
        }

        public void WeakReferenceCollectionInterval(int milliseconds)
        {
            _config.Put(WeakReferenceCollectionIntervalKey, milliseconds);
        }

        public void WeakReferences(bool flag)
        {
            _config.Put(WeakReferencesKey, flag);
        }

        public void AddAlias(IAlias alias)
        {
            if (null == alias)
            {
                throw new ArgumentNullException("alias");
            }
            Aliases().Add(alias);
        }

        public void RemoveAlias(IAlias alias)
        {
            if (null == alias)
            {
                throw new ArgumentNullException("alias");
            }
            Aliases().Remove(alias);
        }

        public IDiagnosticConfiguration Diagnostic()
        {
            return (IDiagnosticConfiguration) _config.Get(DiagnosticKey);
        }

        public bool InternStrings()
        {
            return _internStrings;
        }

        public void RecoveryMode(bool flag)
        {
            _config.Put(RecoveryModeKey, flag);
        }

        public IoAdapter Io()
        {
            throw new NotImplementedException();
        }

        public IStorage Storage
        {
            get { return (IStorage) _config.Get(StorageFactoryKey); }
            set
            {
                var factory = value;
                _config.Put(StorageFactoryKey, factory);
            }
        }

        public IQueryConfiguration Queries()
        {
            return this;
        }

        public IClientServerConfiguration ClientServer()
        {
            return this;
        }

        public void RegisterTypeHandler(ITypeHandlerPredicate predicate, ITypeHandler4 typeHandler
            )
        {
            if (_registeredTypeHandlers == null)
            {
                _registeredTypeHandlers = new Collection4();
            }
            _registeredTypeHandlers.Add(new TypeHandlerPredicatePair(predicate, typeHandler));
        }

        public ICacheConfiguration Cache()
        {
            return new CacheConfigurationImpl(this);
        }

        public int MaxStackDepth()
        {
            return _config.GetAsInt(MaxStackDepthKey);
        }

        public void MaxStackDepth(int maxStackDepth)
        {
            _config.Put(MaxStackDepthKey, maxStackDepth);
        }

        public object DeepClone(object param)
        {
            var ret = new Config4Impl();
            var context = new ConfigDeepCloneContext
                (this, ret);
            ret._config = (KeySpecHashtable4) _config.DeepClone(context);
            ret._internStrings = _internStrings;
            ret._messageLevel = _messageLevel;
            ret._readOnly = _readOnly;
            if (_registeredTypeHandlers != null)
            {
                ret._registeredTypeHandlers = (Collection4) _registeredTypeHandlers.DeepClone(context
                    );
            }
            return ret;
        }

        public void DiscardSmallerThan(int byteCount)
        {
            if (byteCount < 0)
            {
                throw new ArgumentException();
            }
            _config.Put(DiscardFreespaceKey, byteCount);
        }

        public void FreespaceFiller(IFreespaceFiller freespaceFiller)
        {
            _config.Put(FreespaceFillerKey, freespaceFiller);
        }

        public void UseBTreeSystem()
        {
            _config.Put(FreespaceSystemKey, AbstractFreespaceManager.FmBtree);
        }

        public void UseRamSystem()
        {
            _config.Put(FreespaceSystemKey, AbstractFreespaceManager.FmRam);
        }

        [Obsolete]
        public void UseIndexSystem()
        {
            throw new NotSupportedException();
        }

        /// <summary>The ConfigImpl also is our messageSender</summary>
        public void Send(object obj)
        {
            if (_container != null)
            {
                _container.Send(obj);
            }
        }

        public void EvaluationMode(QueryEvaluationMode mode)
        {
            _config.Put(QueryEvaluationModeKey, mode);
        }

        public QueryEvaluationMode EvaluationMode()
        {
            return (QueryEvaluationMode) _config.Get(QueryEvaluationModeKey);
        }

        /// <summary>
        ///     Returns an iterator for all
        ///     <see cref="Db4objects.Db4o.Config.IConfigurationItem">
        ///         Db4objects.Db4o.Config.IConfigurationItem
        ///     </see>
        ///     instances
        ///     added.
        /// </summary>
        /// <seealso cref="Add(Db4objects.Db4o.Config.IConfigurationItem)">
        ///     Add(Db4objects.Db4o.Config.IConfigurationItem)
        /// </seealso>
        /// <returns>the iterator</returns>
        public IEnumerator ConfigurationItemsIterator()
        {
            var items = ConfigurationItems();
            if (items == null)
            {
                return Iterators.EmptyIterator;
            }
            return items.Keys();
        }

        private Hashtable4 SafeConfigurationItems()
        {
            var items = ConfigurationItems();
            if (items == null)
            {
                items = new Hashtable4(16);
                _config.Put(ConfigurationItemsKey, items);
            }
            return items;
        }

        private Hashtable4 ConfigurationItems()
        {
            return (Hashtable4) _config.Get(ConfigurationItemsKey);
        }

        public void ApplyConfigurationItems(IInternalObjectContainer container)
        {
            var items = ConfigurationItems();
            if (items == null)
            {
                return;
            }
            var i = items.Iterator();
            while (i.MoveNext())
            {
                var entry = (IEntry4) i.Current;
                var item = (IConfigurationItem) entry.Value();
                item.Apply(container);
            }
        }

        public void CallbackMode(CallBackMode mode)
        {
            _config.Put(CallbacksKey, mode);
        }

        public Config4Class ConfigClass(string className)
        {
            var config = (Config4Class) ExceptionalClasses().Get(className);
            return config;
        }

        private bool IsIgnoredClass(string className)
        {
            var ignore = IgnoredClasses();
            for (var i = 0; i < ignore.Length; i++)
            {
                if (ignore[i].FullName.Equals(className))
                {
                    return true;
                }
            }
            return false;
        }

        private Type[] IgnoredClasses()
        {
            return new[] {typeof (StaticClass), typeof (StaticField)};
        }

        public void Container(ObjectContainerBase container)
        {
            _container = container;
        }

        public int DatabaseGrowthSize()
        {
            return _config.GetAsInt(DatabaseGrowthSizeKey);
        }

        internal void OldEncryptionOff()
        {
            _config.Put(EncryptKey, false);
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal void EnsureDirExists(string path)
        {
            var file = new File(path);
            if (!file.Exists())
            {
                file.Mkdirs();
            }
            if (file.Exists() && file.IsDirectory())
            {
            }
            else
            {
                throw new IOException(Messages.Get(37, path));
            }
        }

        internal TextWriter ErrStream()
        {
            var outStream = OutStreamOrNull();
            return outStream == null ? Runtime.Err : outStream;
        }

        public IFreespaceFiller FreespaceFiller()
        {
            return (IFreespaceFiller) _config.Get(FreespaceFillerKey);
        }

        private void GlobalSettingOnly()
        {
            if (_container != null)
            {
                throw new GlobalOnlyConfigException();
            }
        }

        private TextWriter OutStreamOrNull()
        {
            return (TextWriter) _config.Get(OutstreamKey);
        }

        public TextWriter OutStream()
        {
            var outStream = OutStreamOrNull();
            return outStream == null ? Runtime.Out : outStream;
        }

        public GenericReflector Reflector()
        {
            var reflector = (GenericReflector) _config.Get(ReflectorKey);
            if (reflector == null)
            {
                var configuredReflector = (IReflector) _config.Get(ConfiguredReflectorKey);
                if (configuredReflector == null)
                {
                    configuredReflector = Platform4.CreateReflector(ClassLoader());
                    _config.Put(ConfiguredReflectorKey, configuredReflector);
                }
                reflector = new GenericReflector(configuredReflector);
                _config.Put(ReflectorKey, reflector);
            }
            // TODO: transaction assignment has been moved to YapStreamBase#initialize1().
            // implement better, more generic solution as described in COR-288
            //		if(! reflector.hasTransaction() && i_stream != null){
            //			reflector.setTransaction(i_stream.getSystemTransaction());
            //		}
            return reflector;
        }

        public void RefreshClasses()
        {
            throw new NotImplementedException();
        }

        internal void Rename(Rename a_rename)
        {
            var renameCollection = Rename();
            if (renameCollection == null)
            {
                renameCollection = new Collection4();
                _config.Put(RenameKey, renameCollection);
            }
            renameCollection.Add(a_rename);
        }

        public IStringEncoding StringEncoding()
        {
            return (IStringEncoding) _config.Get(StringEncodingKey);
        }

        private Collection4 Aliases()
        {
            var aliasesCollection = (Collection4) _config.Get(AliasesKey);
            if (null == aliasesCollection)
            {
                aliasesCollection = new Collection4();
                _config.Put(AliasesKey, aliasesCollection);
            }
            return aliasesCollection;
        }

        public string ResolveAliasRuntimeName(string runtimeType)
        {
            var configuredAliases = Aliases();
            if (null == configuredAliases)
            {
                return runtimeType;
            }
            var i = configuredAliases.GetEnumerator();
            while (i.MoveNext())
            {
                var resolved = ((IAlias) i.Current).ResolveRuntimeName(runtimeType);
                if (null != resolved)
                {
                    return resolved;
                }
            }
            return runtimeType;
        }

        public string ResolveAliasStoredName(string storedType)
        {
            var configuredAliases = Aliases();
            if (null == configuredAliases)
            {
                return storedType;
            }
            var i = configuredAliases.GetEnumerator();
            while (i.MoveNext())
            {
                var resolved = ((IAlias) i.Current).ResolveStoredName(storedType);
                if (null != resolved)
                {
                    return resolved;
                }
            }
            return storedType;
        }

        internal IReflectClass ReflectorFor(object clazz)
        {
            return ReflectorUtils.ReflectClassFor(Reflector(), clazz);
        }

        public bool AllowVersionUpdates()
        {
            return _config.GetAsBoolean(AllowVersionUpdatesKey);
        }

        public bool AutomaticShutDown()
        {
            return _config.GetAsBoolean(AutomaticShutdownKey);
        }

        public byte BlockSize()
        {
            return _config.GetAsByte(BlocksizeKey);
        }

        public int BTreeNodeSize()
        {
            return _config.GetAsInt(BtreeNodeSizeKey);
        }

        public string BlobPath()
        {
            return _config.GetAsString(BlobPathKey);
        }

        public CallBackMode CallbackMode()
        {
            return (CallBackMode) _config.Get(CallbacksKey);
        }

        public TernaryBool CallConstructors()
        {
            return _config.GetAsTernaryBool(CallConstructorsKey);
        }

        internal bool ClassActivationDepthConfigurable()
        {
            return _config.GetAsBoolean(ClassActivationDepthConfigurableKey);
        }

        internal object ClassLoader()
        {
            return _config.Get(ClassloaderKey);
        }

        public bool DetectSchemaChanges()
        {
            return _config.GetAsBoolean(DetectSchemaChangesKey);
        }

        public bool CommitRecoveryDisabled()
        {
            return _config.GetAsBoolean(DisableCommitRecoveryKey);
        }

        public DiagnosticProcessor DiagnosticProcessor
            ()
        {
            return (DiagnosticProcessor) _config.Get(DiagnosticKey
                );
        }

        public int DiscardFreeSpace()
        {
            return _config.GetAsInt(DiscardFreespaceKey);
        }

        internal byte Encoding()
        {
            return _config.GetAsByte(EncodingKey);
        }

        internal bool Encrypt()
        {
            return _config.GetAsBoolean(EncryptKey);
        }

        public Hashtable4 ExceptionalClasses()
        {
            var exceptionalClasses = (Hashtable4) _config.Get(ExceptionalClassesKey);
            if (exceptionalClasses == null)
            {
                exceptionalClasses = new Hashtable4(16);
                _config.Put(ExceptionalClassesKey, exceptionalClasses);
            }
            return exceptionalClasses;
        }

        public bool ExceptionsOnNotStorable()
        {
            return _config.GetAsBoolean(ExceptionsOnNotStorableKey);
        }

        internal byte FreespaceSystem()
        {
            return _config.GetAsByte(FreespaceSystemKey);
        }

        public ConfigScope GenerateUUIDs()
        {
            return (ConfigScope) _config.Get(GenerateUuidsKey);
        }

        public TernaryBool GenerateCommitTimestamps()
        {
            return (TernaryBool) _config.Get(GenerateCommitTimestampsKey);
        }

        public bool LockFile()
        {
            return _config.GetAsBoolean(LockFileKey);
        }

        public int MessageLevel()
        {
            return _messageLevel;
        }

        public IMessageRecipient MessageRecipient()
        {
            return (IMessageRecipient) _config.Get(MessageRecipientKey);
        }

        internal bool OptimizeNQ()
        {
            return _config.GetAsBoolean(OptimizeNqKey);
        }

        internal string Password()
        {
            return _config.GetAsString(PasswordKey);
        }

        public int PrefetchIDCount()
        {
            return _config.GetAsInt(PrefetchIdCountKey);
        }

        public int PrefetchObjectCount()
        {
            return _config.GetAsInt(PrefetchObjectCountKey);
        }

        public Hashtable4 ReadAs()
        {
            return (Hashtable4) _config.Get(ReadAsKey);
        }

        public bool IsReadOnly()
        {
            return _readOnly;
        }

        public bool RecoveryMode()
        {
            return _config.GetAsBoolean(RecoveryModeKey);
        }

        internal Collection4 Rename()
        {
            return (Collection4) _config.Get(RenameKey);
        }

        public int ReservedStorageSpace()
        {
            return _config.GetAsInt(ReservedStorageSpaceKey);
        }

        public bool SingleThreadedClient()
        {
            return _config.GetAsBoolean(SingleThreadedClientKey);
        }

        public bool TestConstructors()
        {
            return _config.GetAsBoolean(TestConstructorsKey);
        }

        public int TimeoutClientSocket()
        {
            return _config.GetAsInt(TimeoutClientSocketKey);
        }

        public int TimeoutServerSocket()
        {
            return _config.GetAsInt(TimeoutServerSocketKey);
        }

        public int UpdateDepth()
        {
            return _config.GetAsInt(UpdateDepthKey);
        }

        public int WeakReferenceCollectionInterval()
        {
            return _config.GetAsInt(WeakReferenceCollectionIntervalKey);
        }

        public bool WeakReferences()
        {
            return _config.GetAsBoolean(WeakReferencesKey);
        }

        public void QueryResultIteratorFactory(IQueryResultIteratorFactory factory)
        {
            _config.Put(ClientQueryResultIteratorFactoryKey, factory);
        }

        public IQueryResultIteratorFactory QueryResultIteratorFactory()
        {
            return (IQueryResultIteratorFactory) _config.Get(ClientQueryResultIteratorFactoryKey
                );
        }

        public bool BatchMessages()
        {
            return _config.GetAsBoolean(BatchMessagesKey);
        }

        public int MaxBatchQueueSize()
        {
            return _config.GetAsInt(MaxBatchQueueSizeKey);
        }

        public void ActivationDepthProvider(IActivationDepthProvider provider)
        {
            _config.Put(ActivationDepthProviderKey, provider);
        }

        public void UpdateDepthProvider(IUpdateDepthProvider provider)
        {
            _config.Put(UpdateDepthProviderKey, provider);
        }

        public IActivationDepthProvider ActivationDepthProvider()
        {
            return (IActivationDepthProvider) _config.Get(ActivationDepthProviderKey);
        }

        public IUpdateDepthProvider UpdateDepthProvider()
        {
            return (IUpdateDepthProvider) _config.Get(UpdateDepthProviderKey);
        }

        public ITypeHandler4 TypeHandlerForClass(IReflectClass classReflector, byte handlerVersion
            )
        {
            if (_registeredTypeHandlers == null)
            {
                return null;
            }
            var i = _registeredTypeHandlers.GetEnumerator();
            while (i.MoveNext())
            {
                var pair = (TypeHandlerPredicatePair) i.Current;
                if (pair._predicate.Match(classReflector))
                {
                    return pair._typeHandler;
                }
            }
            return null;
        }

        public void Factory(ILegacyClientServerFactory factory)
        {
            _config.Put(ClientServerFactoryKey, factory);
        }

        public ILegacyClientServerFactory ClientServerFactory()
        {
            return (ILegacyClientServerFactory) _config.Get(ClientServerFactoryKey);
        }

        public bool FileBasedTransactionLog()
        {
            return _config.GetAsBoolean(FileBasedTransactionLogKey);
        }

        public void FileBasedTransactionLog(bool flag)
        {
            _config.Put(FileBasedTransactionLogKey, flag);
        }

        private bool IsTainted()
        {
            return _config.GetAsBoolean(TaintedKey);
        }

        public void Taint()
        {
            _config.Put(TaintedKey, true);
        }

        public static void AssertIsNotTainted(IConfiguration config)
        {
            if (((Config4Impl) config).IsTainted())
            {
                throw new ArgumentException("Configuration already used.");
            }
        }

        private void EnsurePrefetchSlotCacheSize()
        {
            if (!_prefetchSlotCacheSizeModifiedExternally)
            {
                PrefetchSlotCacheSize(CalculatedPrefetchSlotcacheSize());
                _prefetchSlotCacheSizeModifiedExternally = false;
            }
        }

        public int PrefetchDepth()
        {
            return _config.GetAsInt(PrefetchDepthKey);
        }

        public IList EnvironmentContributions()
        {
            return (IList) _config.Get(EnvironmentContributionsKey);
        }

        public int PrefetchSlotCacheSize()
        {
            return _config.GetAsInt(PrefetchSlotCacheSizeKey);
        }

        private int CalculatedPrefetchSlotcacheSize()
        {
            var calculated = (long) PrefetchDepth()*PrefetchObjectCount()*PrefetchSlotCacheSizeFactor;
            if (calculated > MaximumPrefetchSlotCacheSize)
            {
                calculated = MaximumPrefetchSlotCacheSize;
            }
            return (int) calculated;
        }

        public event EventHandler<EventArgs> PrefetchSettingsChanged
        {
            add
            {
                _prefetchSettingsChanged = (EventHandler<EventArgs>) Delegate.Combine
                    (_prefetchSettingsChanged, value);
            }
            remove
            {
                _prefetchSettingsChanged = (EventHandler<EventArgs>) Delegate.Remove
                    (_prefetchSettingsChanged, value);
            }
        }

        public void ReferenceSystemFactory(IReferenceSystemFactory referenceSystemFactory
            )
        {
            _config.Put(ReferenceSystemFactoryKey, referenceSystemFactory);
        }

        public IReferenceSystemFactory ReferenceSystemFactory()
        {
            return (IReferenceSystemFactory) _config.Get(ReferenceSystemFactoryKey);
        }

        public void NameProvider(INameProvider provider)
        {
            _config.Put(NameProviderKey, provider);
        }

        public INameProvider NameProvider()
        {
            return (INameProvider) _config.Get(NameProviderKey);
        }

        public void UsePointerBasedIdSystem()
        {
            _config.Put(IdSystemKey, StandardIdSystemFactory.PointerBased);
        }

        public void UseStackedBTreeIdSystem()
        {
            _config.Put(IdSystemKey, StandardIdSystemFactory.StackedBtree);
        }

        public void UseSingleBTreeIdSystem()
        {
            _config.Put(IdSystemKey, StandardIdSystemFactory.SingleBtree);
        }

        public byte IdSystemType()
        {
            return _config.GetAsByte(IdSystemKey);
        }

        public void UseInMemoryIdSystem()
        {
            _config.Put(IdSystemKey, StandardIdSystemFactory.InMemory);
        }

        public void UseCustomIdSystem(IIdSystemFactory factory)
        {
            _config.Put(IdSystemKey, StandardIdSystemFactory.Custom);
            _config.Put(IdSystemCustomFactoryKey, factory);
        }

        public IIdSystemFactory CustomIdSystemFactory()
        {
            return (IIdSystemFactory) _config.Get(IdSystemCustomFactoryKey);
        }

        public void AsynchronousSync(bool flag)
        {
            _config.Put(AsynchronousSyncKey, flag);
        }

        public bool AsynchronousSync()
        {
            return _config.GetAsBoolean(AsynchronousSyncKey);
        }

        private sealed class _IDeferred_75 : KeySpec.IDeferred
        {
            //  TODO: consider setting default to 8, it's more efficient with freespace.
            public object Evaluate()
            {
                return DefaultClientServerFactory();
            }
        }

        private sealed class _IDeferred_85 : KeySpec.IDeferred
        {
            public object Evaluate()
            {
                return new DiagnosticProcessor();
            }
        }

        private sealed class _IDeferred_103 : KeySpec.IDeferred
        {
            public object Evaluate()
            {
                return new ArrayList();
            }
        }

        private sealed class _IDeferred_155 : KeySpec.IDeferred
        {
            // for playing with different strategies of prefetching
            // object
            public object Evaluate()
            {
                return new Hashtable4(16);
            }
        }

        private sealed class _IReferenceSystemFactory_193 : IReferenceSystemFactory
        {
            public IReferenceSystem NewReferenceSystem(IInternalObjectContainer container)
            {
                return new TransactionalReferenceSystem();
            }
        }

        private sealed class _INameProvider_199 : INameProvider
        {
            public string Name(IObjectContainer db)
            {
                return null;
            }
        }

        public class ConfigDeepCloneContext
        {
            public readonly Config4Impl _cloned;
            public readonly Config4Impl _orig;

            public ConfigDeepCloneContext(Config4Impl orig, Config4Impl cloned)
            {
                _orig = orig;
                _cloned = cloned;
            }
        }
    }
}