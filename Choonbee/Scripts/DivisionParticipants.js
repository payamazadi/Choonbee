var opt = {
    numType: 'absolute',
    numMin: 60,
    numMax: 72,
    //numMinLabel:true,
    //numMaxLabel:true,
    labelVisibility: 'hidden',
    forceAnim: true,
    horTitle: 'bi-data-title',
    horBarHeight: 3,
    milestones: {
        1: {
            mlPos: 20,
            mlId: false,
            mlClass: 'bi-custom',
            mlDim: '200%',
            mlLabelVis: 'hidden',
            mlHoverRange: 15,
            mlLineWidth: 1
        },
    }
};

var activeBuckets = [];
var stagedBuckets = [];

var defaultNames = [[], [], [], [], [], [], [], []];
defaultNames[0] = [""];
defaultNames[1] = ["Light", "Heavy"];
defaultNames[2] = ["Light", "Middle", "Heavy"];
defaultNames[3] = ["Bantam", "Light", "Middle", "Heavy"];
defaultNames[4] = ["Flyweight", "Bantam", "Light", "Middle", "Heavy"];
defaultNames[5] = ["Flyweight", "Bantam", "Superlight", "Light", "Middle", "Heavy"];
defaultNames[6] = ["Flyweight", "Bantam", "Superlight", "Light", "Light Middle", "Middle", "Heavy"];
defaultNames[7] = ["Flyweight", "Bantam", "Superlight", "Light", "Light Middle", "Middle", "Light Heavy", "Heavy"];


var minHeight = 0;
var maxHeight = 0;
var heightOffset = 0;
var division = BindDivision();

$("#participantKiller").sortable({ connectWith: ".connectedSortable" });
$("#participantKiller").on("sortreceive", function (event, ui) {
    $("#participant" + ui.item.attr("participantid")).remove();
});


window.addEventListener("beforeunload", function (e) {
    var confirmationMessage = 'If you leave this page and then attempt to come back to it, all your work will be gone. Only navigate away when done staging. Otherwise, open whatever you need in a new tab.';

    (e || window.event).returnValue = confirmationMessage; //Gecko + IE
    return confirmationMessage; //Gecko + Webkit, Safari, Chrome etc.
});


createDefaultBucket(division);


function BindDivision() {
    var division = new Division();
    
    division.DivisionId = 0;
    division.TournamentId = $("#TournamentId").val();
    division.ParentDivisionId = $("#ParentDivisionId").val();
    division.FriendlyId = $("#FriendlyId").val();
    division.RankGroupId = $("#RankGroupId").val();
    division.AgeGroupId = $("#AgeGroupId").val();
    division.DivisionTypeId = $("#DivisionTypeId").val();
    division.Genders = $("#Genders").val();
    division.AdditionalidentifierText = $("#AdditionalidentifierText").val();
    division.HadTieBreaker = $("#HadTieBreaker").val();
    division.DivisionStatusId = $("#DivisionStatusId").val();
    division.DateEntered = $("#DateEntered").val();
    division.Order = $("#Order").val();
    division.NumSplits = $("#NumSplits").val();

    return division;
}

function getParticipants(thecallback) {
    $.ajax({
        type: 'POST',
        url: '/Division/GetParticipants/' + $("#DivisionId").val(),
        traditional: true,
        success: thecallback
    });
}

function createDefaultBucket(division) {
    getParticipants(function (response) {
        var participants = JSON.parse(response);
        var defaultBucket = new DefaultBucket(participants, division, 0);
        defaultBucket.division.ParentDivisionId = $("#DivisionId").val();
        opt.numMin = defaultBucket.getMinHeight();
        opt.numMax = defaultBucket.getMaxHeight();
        injectActiveBucket(defaultBucket);
    });
}

function injectActiveBucket(bucket) {
    var bucketHtml = bucket.getHtml();
    $("#mainContainer").append(bucketHtml);
    activeBuckets.push(bucket);
}

function splitBucket(id) {
    var newBuckets = activeBuckets[id].split(activeBuckets.length);
    jQuery.each(newBuckets, function (idx, item) {
        injectActiveBucket(item);
    });

    updateActiveNames();
}

function updateActiveNames() {
    defaults = defaultNames[Math.min(activeBuckets.length - 2, defaultNames.length-1)];
    jQuery.each(activeBuckets, function (idx, item) {
        if (idx != 0) {
            if (idx >= activeBuckets.length-1)
                defaults.push("Extra" + idx);
            $("#bucket" + idx + " .divisionHeading").html(defaults[idx - 1]);
            activeBuckets[idx].division.FriendlyId += " " + defaults[idx - 1];
        }
    });
}

function stage(id) {
    //log all the division details
    //ajax call to create the division and register participants
    $.ajax({
        type: 'POST',
        url: '/Division/CreateDivision',
        traditional: true,
        data: { thedivision: JSON.stringify(activeBuckets[id].division), theparticipantlist: JSON.stringify(activeBuckets[id].participants) },
        success: function (response) {
            var returned = JSON.parse(response);
            returned = returned.DivisionId;
            $.featherlight({
                iframe: '/Division/ManageBracket/' + returned, /*iframeMaxWidth: '80%',*/ iframeWidth: 800, iframeHeight: 600, afterClose: function handleClosedBracket(event) {
                    if ($("#Successful").val() == "true") {
                        stageBucket(id);
                        $("#Successful").val("false");
                    }
                    else {
                    }
                    
                }
            });
        }
    });
}

function stageBucket(id) {
    var bucket = activeBuckets[id];
    var temp = $("#bucket" + id).detach();
    $("#secondaryContainer").append(temp);

    bucket.id = stagedBuckets.length;
    stagedBuckets.push(bucket);
    refreshBucketIds(id, "staged", bucket.id);

    $("#stagedutilLinks" + bucket.id).empty();
    activeBuckets.splice(id, 1);
    reindex(activeBuckets);
}

function reindex(bucket) {
    jQuery.each(bucket, function (idx, item) {
        refreshBucketIds(item.id, "", idx);
        item.id = idx;
        $("#utilLinks" + idx).html(item.getUtilLinks());
    });
}

function refreshBucketIds(oldid, prepend, newid) {
    $("#bucket" + oldid).attr("id", prepend + "bucket" + newid);
    $("#sortable" + oldid).attr("id", prepend + "sortable" + newid);
    $("#utilLinks" + oldid).attr("id", prepend + "utilLinks" + newid);
}
//inject buckets into DOM
//manage buckets, opt, minHeight etc variables
//pass global data into models appropriately
//provide anything necessary for view (.cs file)

function addBucket() {
    var bucket = new Bucket([], jQuery.extend(true, {}, activeBuckets[0].division), activeBuckets.length);
    injectActiveBucket(bucket);
    updateClassHelper(bucket.id, prompt("Class name?"));
}

function updateClass(id, type) {
    switch (type) {
        case 'F':
            updateClassHelper(id, "Flyweight");
            break;
        case 'B':
            updateClassHelper(id, "Bantamweight");
            break;
        case 'SL':
            updateClassHelper(id, "Superlight");
            break;
        case 'L':
            updateClassHelper(id, "Light");
            break;
        case 'LM':
            updateClassHelper(id, "Light Middle");
            break;
        case 'M':
            updateClassHelper(id, "Middle");
            break;
        case 'LH':
            updateClassHelper(id, "Light Heavy");
            break;
        case 'H':
            updateClassHelper(id, "Heavy");
            break;
        case 'O':
            updateClassHelper(id, prompt("Custom name?"));
            break;
        default:
            alert("noop");
    }
}

function updateClassHelper(id, name) {
    $("#bucket" + id + " .divisionHeading").html(name);
    activeBuckets[id].division.FriendlyId = $("#FriendlyId").val() + " " + name;
}

function refreshParticipants() {
    getParticipants(function (response) {
        var participants = JSON.parse(response);
        for (var i = 0; i < participants.length; i++) {
            if (!$("#participant" + participants[i].ParticipantId).length) {
                activeBuckets[0].injectParticipant(participants[i], $("#sortable0"), true);
            }
        }
    });
}

function addAdditionalParticipant() {
    $.ajax({
        type: 'POST',
        url: '/Participant/GetParticipant/' + $("#AllParticipants").val(),
        traditional: true,
        success: function (response) {
            var participant = JSON.parse(response)[0];
            activeBuckets[0].injectParticipant(participant, $("#sortable0"), true);
        }
    });

    //GetParticipant
}

function sortBucket(id) {
    activeBuckets[id].sortParticipants();
}

function destroyBucket(id) {
    //move participants
    //splice from array
    //remove html
    //reindex
    

    var bucket = activeBuckets[id];
    jQuery.each(bucket.participants, function (idx, item) {
        activeBuckets[0].injectParticipant(item, $("#sortable0"), true);
    });

    $("#bucket" + id).remove();

    activeBuckets.splice(id, 1);
    reindex(activeBuckets);
}