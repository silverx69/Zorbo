Script.create("room-eval");

print("Room javascript evaluator loaded!");

function onBeforePacket(userobj, jsonpacket) {
    packet = JSON.parse(jsonpacket);
    if (packet.id == 10) {
        var txt = packet.message;
        if (userobj.admin >= AdminLevel.Admin && txt.length > 0) {

            var priv = txt[0] == "@";
            if (priv) txt = txt.substr(1);

            txt = "try { userobj = user(" + userobj.id + ");null;" + txt + "} catch(ex) { }";

            var ret = Script.eval("room-eval", txt);
            if (ret != null && ret !== 'undefined') {
                if (priv)
                    print(userobj, ret);
                else
                    print(userobj.vroom, ret);
            }

            if (priv) return false;
        }
    }
    return true;
}