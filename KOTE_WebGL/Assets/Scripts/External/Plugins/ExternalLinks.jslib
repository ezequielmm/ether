var plugin = {
    openWindow: function(link)
    {
        var url = Pointer_stringify(link);
       document.onmouseup = function()
       {
           window.open(url);
           document.onmouseup = null;
       }   
    }
};