mergeInto(LibraryManager.library, {
    GetUnityMessage: function (message) {
      try {
        window.dispatchReactUnityEvent("GetUnityMessage", message);
      } catch (e) {
        console.error("Failed to dispatch event");
      }
    },
  });