


async function DoCallback(action, o, destURL, aft) {
    var CTS = new Object();
    CTS.BBPAddress = "";
    CTS.ExtraData = JSON.stringify(o);
    CTS.FormData = MemorizeForm();
    CTS.Action = action;

    CTS.RVT = "";
    var o = document.getElementsByName("__RequestVerificationToken");
    if (o && o.length > 0) {
        CTS.RVT = o[0].value;
        console.log('1'+CTS.RVT);
    }

    var postURL = 'admin/processdocallback';
    if (destURL != null && destURL != '') {
        postURL = destURL;
    }
    console.log('1 ' + postURL);
    console.log('2 ' + destURL);
    console.log(CTS);
    await $.ajax({

        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", CTS.RVT)
        },

        type: "POST",
        url: postURL,
        data: JSON.stringify(CTS)
        ,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data, resObject) {
            if (data != null && data.length > 1)
            {
                //console.log('data[] ' + data);
                var obj = JSON.parse(data);
                if (obj.returntype == "modal") {
                    var implant = document.getElementById("implant");

                    try {
                        removeAllChildNodes(implant);
                    }
                    catch (e) {

                    }
                    var div = document.createElement('div');
                    div.innerHTML = obj.returnbody;
                    implant.appendChild(div);
                    $('#modalid1').modal('show');
                } else if (obj.returntype == "javascript") {
                    setTimeout(obj.returnbody, 1);
                }
            }
        },
        error: function (e)
        {
            console.log(e);
            console.log('Error while inserting data');
        }
    });
    return true;
}


function DoPostback(URL, EventName, o) {
    var CTS = new Object();
    CTS.BBPAddress = "";
    CTS.ExtraData = JSON.stringify(o);
    CTS.FormData = MemorizeForm();
    CTS.Action = EventName;
    $.ajax({
        type: "POST",
        url: URL,
        data: JSON.stringify(CTS),
        dataType: "json",
        contentType: "application/json",
        success: function (response)
        {
           console.log(response);
           if (response.type == 'javascript') {
                eval(response.body);
            }
        },
        error: function () {
            alert('Error while posting data to the server.');
        }
    });
    return true;
}

function getUnixTime() {
    return Math.floor(Date.now() / 1000)
}

var nMyLastLog = 0;
function BackgroundPost(oMyLogger) {
    var nElapsed = getUnixTime() - nMyLastLog;
    if (nElapsed < 60) {
        return;
    }
    

    if (oMyLogger.length > 1000) {
        nMyLastLog = getUnixTime();

        var e = {};
        e.Log = oMyLogger;
        //DoCallback('Log_Save', e, 'paginator/processdocallback');
        var CTS = new Object();
        CTS.BBPAddress = "";
        CTS.ExtraData = oMyLogger;
        CTS.FormData = '';// MemorizeForm();
        CTS.Action = 'Log_Save';

        $.ajax({
            type: "POST",
            url: '../../paginator/processdocallback',
            data: JSON.stringify(CTS),
            dataType: "json",
            contentType: "application/json",
            success: function (response) {
            },
            error: function () {
                //alert('Error while posting data to the server.');
            }
        });


        oMyLogger = '';
    }
}

// Global Error Handler - Future
// Global Logging Handler

var mylogger = '';
(function () {
    if (false) {
        var oldLog = console.log;
        console.log = function (message) {
            // Implement centralized js logger here, if desired.
            mylogger += JSON.stringify(message) + '\r\n';
            BackgroundPost(mylogger);
            oldLog.apply(console, arguments);
        };
    }
})();



function setCookie(name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}
function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}
function eraseCookie(name) {
    document.cookie = name + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
}


function ElementsToHTML(sType) {
    var elements = document.getElementsByTagName(sType);

    var html = "";
    for (var i = 0; i < elements.length; i++) {
        var id = elements[i].id;
        var value = elements[i].value;
        var parentid = elements[i].getAttribute('data-parentid');

        if (elements[i].type == 'radio' || elements[i].type == 'checkbox')
        {
            console.log('checkbox ' + id);
            if (!elements[i].checked) {
                value = "false";
            } else {
                value = "true"
            }
        }
        if (id && value) {
            var row = parentid + "<col>" + id + "<col>" + value + "<row>";
            html += row;
        }
    }
    var o1 = document.getElementById('divPaste');
    if (o1) {
        var row = "<col>divPaste<col>" + o1.innerHTML + "<row>";
        html += row;
    }
    return html;
}

function MemorizeForm() {
    var html = ElementsToHTML("input");
    var ta = ElementsToHTML("textarea");
    var td = ElementsToHTML("select");
    console.log(html);
    console.log(ta);
    console.log(td);

    var h = html + ta + td;
    return h;
}

function openModal(id) {
    document.getElementById(id).style.display = "block";
    document.getElementById(id).classList.add("show");
}

function closeModal(id) {
    $('#' + id).hide();
    $('#' + id).modal('hide');
}

function closeModalByDataTarget(id) {
    var oID = document.getElementById(id);
    //    var targetElement = document.getElementById(obj.getAttribute('data-target'));
    oID.classList.toggle('tooltip');
}


function removeAllChildNodes(parent) {
    while (parent.firstChild) {
        parent.removeChild(parent.firstChild);
    }
}


// Infinite scrolling paginator; start at record 29
var iGallery = 29;
function MakeMoreVisible() {
    for (var i = iGallery; i < iGallery + 30; i++) {
        $('#gtd' + i.toString()).toggleClass('galleryinvisible');
    }
    iGallery += 30;
}

function getParameterByName(name, url = window.location.href) {
    name = name.replace(/[\[\]]/g, '\\$&');
    var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, ' '));
}




// Scroll event listener
window.addEventListener('scroll', () => {
    const {
        scrollTop,
        scrollHeight,
        clientHeight
    } = document.documentElement;

    var signalType = 0;
    if (scrollTop === 0) {
        signalType = 1;
    }

    if (scrollTop + clientHeight >= scrollHeight - 50) {
        signalType = 2;
    }

    if (signalType !== 0) {
        var pag = "0" + getParameterByName('pag');
        var nPag = parseInt(pag);
        var url = window.location.href;
        var nOffset = 0;

        if (signalType === 1) {
            nOffset = -30;
        }
        else if (signalType === 2) {
            nOffset = 30;
        }

        if (url.includes('TelegramChat') && signalType === 1) {
            nOffset = 30;
        }

        var nNew = nPag + nOffset;
        if (nNew < 1)
            nNew = 1;
        // First if this param exists; remove
        if (nPag > 0) {
            var sOld = "pag=" + (nPag).toString();

            var sNew = "pag=" + (nNew).toString();
            url = url.replace(sOld, sNew);
        }
        else {
            var fQ = url.includes('?');
            url += fQ ? '&pag=' : '?pag=';
            url += (nNew).toString();
        }
        // If the paginator is enabled for the page:
        if (nPag < 2 && nNew < 2) {
            // Noop
            return;
        }
        if (url.includes('videolist')) {
            window.location.href = url;
        }
    }
}, {
    passive: true
});

