function Sock(url) {
    this.url = url;
    this.sock = null;
    this.connected = false;
    this.timeout = null;
    this.receive = null;
    
    this.reset();
    this.open();
}

Sock.prototype = {
    open: function () {
        var me = this;

        if (me.sock) {
            me.sock.onclose = null;
            me.sock.close();
        }

        var sock = me.sock = new WebSocket(me.url);

        sock.onopen = function (e) {
            me.reset();
            me.connected = true;
            me.onopen && me.onopen(e);
        };

        sock.onerror = function (e) {
			console.log(e);
            sock.close();
        }

        sock.onclose = function (e) {
            me.connected = false;
            me.retry && me.retry();
            me.onclose && me.onclose(e);
        }

        sock.onmessage = function (evt) {
			console.log(evt);
            me.onmessage && me.onmessage(evt.data);
        }
    },

    send: function (obj) {
        this.sock.send(JSON.stringify(obj));
    },

    close: function () {
        this.retry = null;
        this.sock && this.sock.close();
    },

    reset: function () {
        this.retry = this._retryLoop;
        this.retryMs = 500;
    },

    _retryLoop: function () {
        var me = this;
        window.clearTimeout(me.timeout);
        me.timeout = window.setTimeout(function () {
            me.retryMs = Math.min(15000, me.retryMs += 500);
            me.open();
        }, me.retryMs);
    }
};


$(function () {

    function status(msg) {
        var d = $('<div />');
        d.text(msg);
        $('#status').append(d);
    }

    var app = new Sock('ws://localhost:8881');
    
	console.info(app);
	
    app.sock.onopen = function () {
        $('#status').html(' ');
        status('Ready');
    };

    app.sock.onclose = function () {
        status('Closed');
    };
	
	app.sock.onmessage = function (evt) {
        status(evt.data);
		console.log(evt.data);
    };
	
    $('#sendBtn').bind('click',function () {
        var message = $('#message').val();
		var msgList=[{
			ID:"1",
			PrintCode:message
		}];
		
		var msg = 
		{		
			msgs:msgList
		};
		
        app.sock.send(JSON.stringify(msg));
    })
});