mergeInto(LibraryManager.library, {
  $ClickDungeonPersistentDataSyncState: {
    status: 0
  },

  ClickDungeonSyncPersistentData__deps: ['$ClickDungeonPersistentDataSyncState'],
  ClickDungeonSyncPersistentData: function () {
    ClickDungeonPersistentDataSyncState.status = 1;
    if (typeof FS === 'undefined' || !FS.syncfs) {
      ClickDungeonPersistentDataSyncState.status = 3;
      return;
    }
    FS.syncfs(false, function (err) {
      if (err) {
        ClickDungeonPersistentDataSyncState.status = 3;
        console.error('ClickDungeon IndexedDB sync failed', err);
      } else {
        ClickDungeonPersistentDataSyncState.status = 2;
      }
    });
  },

  ClickDungeonGetPersistentDataSyncStatus__deps: ['$ClickDungeonPersistentDataSyncState'],
  ClickDungeonGetPersistentDataSyncStatus: function () {
    return ClickDungeonPersistentDataSyncState.status;
  }
});
