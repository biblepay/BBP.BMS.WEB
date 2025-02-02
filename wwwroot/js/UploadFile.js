
function UploadFile() {
    var file = document.getElementById('file').files[0];
    var fd = new FormData();
    fd.append("file", file);
    var xhr = new XMLHttpRequest();
    xhr.upload.addEventListener("progress", function (evt) { UploadProgress(evt); }, false);
    xhr.addEventListener("load",  function (evt) { UploadComplete(evt); }, false);
    xhr.addEventListener("error", function (evt) { UploadFailed(evt);   }, false);
    xhr.addEventListener("abort", function (evt) { UploadCanceled(evt); }, false);
    console.log('posting');
    var parentid = getParameterByName('parentid');
    xhr.open("POST", "Attachment/UploadFileAttachment?parentid=" + parentid, true);
    console.log('posting2');
    xhr.send(fd);
};


function UploadProgress(evt) {
    if (evt.lengthComputable) {
        var percentComplete = Math.round(evt.loaded * 100 / evt.total);
        $("#uploading").text(percentComplete + "% ");
    }
}

function UploadComplete(evt) {
    if (evt.target.status == 200) {
        console.log(evt.target.responseText);
        // This object contains the return value
        if (evt.target.responseText != null && evt.target.responseText.length > 1) {
            var ob1 = JSON.parse(evt.target.responseText);
            var obj = JSON.parse(ob1);
            if (obj.returntype == "uploadsuccess") {
            }
            else if (obj.returntype == "uploadsuccessredirect") {
                location.reload();
            }
            else if (obj.returntype == "modal") {
                var implant = document.getElementById("implant");
                try {
                    removeAllChildNodes(implant);
                }
                catch (e) {
                    console.log(e);
                }
                var div = document.createElement('div');
                div.innerHTML = obj.returnbody;
                implant.appendChild(div);
                $('#modalid1').modal('show');
            }
        }
    }
    else {
        console.log('euf');
        alert("Error Uploading File");
    }
}

function UploadFailed(evt) {
    alert("There was an error attempting to upload the file.");
}

function UploadCanceled(evt) {
    alert("The upload has been canceled by the user or the browser dropped the connection.");
}