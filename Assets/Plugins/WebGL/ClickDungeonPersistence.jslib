mergeInto(LibraryManager.library, {
  ClickDungeonSyncPersistentData: function () {
    if (typeof FS === 'undefined' || !FS.syncfs) return;
    FS.syncfs(false, function (err) {
      if (err) console.error('ClickDungeon IndexedDB sync failed', err);
    });
  }
});
