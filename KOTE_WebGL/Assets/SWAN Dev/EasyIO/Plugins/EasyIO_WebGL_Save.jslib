var EasyIO_WebGL_Save = {

    EasyIO_WebGL_WindowAlert: function (message) {
        window.alert(Pointer_stringify(message));
    },
    
    EasyIO_WebGL_SyncFiles: function () {
        FS.syncfs(false, function (err) {
            // handle callback
        });
    },
    
    EasyIO_WebGL_SaveToLocal: function (array, size, fileNamePtr) {
        console.log(array);
        console.log(size);
        console.log(fileNamePtr);
        
        var fileName = UTF8ToString(fileNamePtr);

        var bytes = new Uint8Array(size);
        for (var i = 0; i < size; i++)
        {
            bytes[i] = HEAPU8[array + i];
        }

        var blob = new Blob([bytes]);
        var link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = fileName;

        var event = document.createEvent("MouseEvents");
        event.initMouseEvent("click");
        link.dispatchEvent(event);
        window.URL.revokeObjectURL(link.href);

        console.log(link);
    }
};

mergeInto(LibraryManager.library, EasyIO_WebGL_Save);