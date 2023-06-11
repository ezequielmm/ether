mergeInto(LibraryManager.library, {
  GetUnityMessage: function (eventName, data) {
    window.dispatchReactUnityEvent("GetUnityMessage", UTF8ToString(eventName), UTF8ToString(data));
  },
});