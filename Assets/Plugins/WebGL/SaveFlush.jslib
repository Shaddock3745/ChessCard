mergeInto(LibraryManager.library, {
  SyncFiles: function () {
    FS.syncfs(false, function (err) {
      if (err) console.error("syncfs error:", err);
    });
  }
});
