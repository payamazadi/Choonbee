var globalMinHeight = 0;
var movedParticipant;

function getCompleteHeight(feet, inches) {
    return (feet * 12 + inches);
}

function getOffsetHeight(feet, inches) {
    return getCompleteHeight(feet, inches) - globalMinHeight;
}

function indexOf(arr, item, equalsFunction) {
    for (var i = 0; i < arr.length; i++)
        if (equalsFunction(arr[i], item))
            return i;
    return -1;
}

function participantMatchesId(participant, id) {
    return participant.ParticipantId == id;
}

function bucketMatchesId(bucket, id) {
    return bucket.id == id;
}

function participantComparison(a, b) {
    var difference = getOffsetHeight(a.HeightFeet, a.HeightInches) - getOffsetHeight(b.HeightFeet, b.HeightInches);
    return difference == 0 ? (a.Age - b.Age) : difference;
}

function Division() {
    this.DivisionId = 0;
    this.TournamentId = 0;
    this.ParentDivisionId = 0;
    this.FriendlyId = "";
    this.RankGroupId = 0;
    this.AgeGroupId = 0;
    this.DivisionTypeId = 0;
    this.Genders = "";
    this.AdditionalidentifierText = "";
    this.HadTieBreaker = false;
    this.DivisionStatusId = 1;
    this.DateEntered = "";
    this.Order = 0
    this.NumSplits = 0;
}

function Bucket(participants, division, id) {
    var that = this;
    this.division = division;
    this.id = id;
    this.participants = participants;
    
    this.heading = "Participants";
    this.getUtilLinks = function () {
        var ret = "<a href='#' onclick='stage(" + this.id + ");'>Stage</a> <a href='#' onclick='sortBucket(" + this.id + ");'>Sort</a> <a href='#' onclick='destroyBucket(" + this.id + ");'>Destroy</a> <br>";
        //defaultNames[7] = ["Flyweight", "Bantam", "Superlight", "Light", "Light Middle", "Middle", "Light Heavy", "Heavy"];
        ret += '<a href="#" onclick="updateClass(' + this.id + ',\'F\');">F</a>';
        ret += '<a href="#" onclick="updateClass(' + this.id + ',\'B\');">B</a>';
        ret += '<a href="#" onclick="updateClass(' + this.id + ',\'SL\');">SL</a>';
        ret += '<a href="#" onclick="updateClass(' + this.id + ',\'L\');">L</a>';
        ret += '<a href="#" onclick="updateClass(' + this.id + ',\'LM\');">LM</a>';
        ret += '<a href="#" onclick="updateClass(' + this.id + ',\'M\');">M</a>';
        ret += '<a href="#" onclick="updateClass(' + this.id + ',\'LH\');">LH</a>';
        ret += '<a href="#" onclick="updateClass(' + this.id + ',\'H\');">H</a>';
        ret += '<a href="#" onclick="updateClass(' + this.id + ',\'O\');">O</a>';
        
        return ret;
    };

    this.getHtml = function () {
        var container = $("<div></div>", { id: "bucket" + this.id });
        container.addClass("col-md-3 bucket");

        if (this.id % 4 == 0)
            container.css("clear", "left");

        var heading = $("<h3></h3>");
        heading.addClass("divisionHeading");
        heading.html(this.heading);
        container.append(heading);

        var utilLinks = $("<div></div>", { id: "utilLinks" + this.id });
        utilLinks.addClass("bucketLinks");
        utilLinks.html(this.getUtilLinks());
        container.append(utilLinks);

        var sortable = $("<ul></ul>", { id: "sortable" + this.id, bucketid: this.id });
        sortable.addClass("connectedSortable");
        container.append(sortable);

        sortable.sortable({ connectWith: ".connectedSortable" });
        

        for (var i = 0; i < this.participants.length; i++) {
            this.injectParticipant(this.participants[i], sortable, false);
        }

        this.sortable = sortable;
        this.sortable.on("sortreceive", this.received);
        this.sortable.on("sortremove", this.removed);

        return container;
    }

    this.injectParticipant = function (participant, sortable, addToList) {
        var participantItem = this.getParticipantHtml(participant);
        sortable.append(participantItem);

        if (addToList)
            this.participants.push(participant);
    }

    this.getParticipantHtml = function (participant) {
        var length = getOffsetHeight(participant.HeightFeet, participant.HeightInches);
        var lengthTitle = participant.HeightFeet + "'" + participant.HeightInches;

        //var length2 = length + 60;
        var sortitem = $("<li></li>", { id: "participant" + participant.ParticipantId, participantid: participant.ParticipantId });

        var bucketSpan = $("<span></span>", { id: "bar" + participant.ParticipantId });
        bucketSpan.attr("data-title", "<span class='gender " + participant.Gender + "'>&nbsp;</span>(<i>" + participant.SchoolName.substring(0,10) + "</i>) <b>" + participant.FirstName + " " + participant.LastName + "</b> (" + participant.Age + ") " + lengthTitle + "<div class='belt belt" + participant.BeltName + "'>&nbsp;</div>");
        bucketSpan.html(length);

        sortitem.append(bucketSpan);
        
        bucketSpan.barIndicator(opt);
        return sortitem;
    }

    this.split = function (nextId) {
        //should this return a list of buckets, or just directly inject them?

        //make decision: split, inject, and remove this, or split and inject?
        //determine number of buckets to split into
        //determine a new identifier
        //add/subtract participants
        //instantiate buckets
        
        var bucketList = [];
        var participantIdsToRemove = [];

        var numPerDivision = getNumPerDivision();

        for (var i = 0; i < this.participants.length; i += numPerDivision) {
            var newBucket = new Bucket();
            var newParticipants = [];
            newBucket.division = jQuery.extend(true, {}, this.division);
            //newBucket.division.ParentDivisionId = buckets[0].division.DivisionId;
            newBucket.division.DivisionStatusId = 1;
            newBucket.division.HadTieBreaker = false;
            newBucket.id = nextId;
            nextId++;

            var shortest = null;
            for (var k = 0; k < numPerDivision; k++) {
                if (i + k < participants.length) {
                    if (getCompleteHeight(this.participants[i+k].HeightFeet, this.participants[i+k].HeightInches) - getCompleteHeight(this.participants[i].HeightFeet, this.participants[i].HeightInches) <= 4 ){
                        newParticipants.push(this.participants[i + k]);
                        participantIdsToRemove.push(this.participants[i + k].ParticipantId);
                    }
                    else {
                        i-= numPerDivision-k;
                        break;
                    }
                }
            }
            newBucket.participants = newParticipants;
            bucketList.push(newBucket);
        }

        for (var i = 0; i < participantIdsToRemove.length; i++) {
            var idx = indexOf(this.participants, participantIdsToRemove[i], function (a, b) { return a == b; });
            this.participants.splice(idx);

            var thisSortable = $("#sortable" + this.id);
            thisSortable.children('li').each(function (){
                if($(this).attr("participantid") == participantIdsToRemove[i])
                    $(this).remove();
            });
        }

        return bucketList;
    }


    function getNumPerDivision(){
        var participantsLength = participants.length;
        var numPerDivision = 0;

        switch (true) {
            case (participantsLength <= 7 && participantsLength > 4):
                numPerDivision = 3;
                break;
            case (participantsLength >= 8 && participantsLength < 12):
                numPerDivision = 4;
                break;
            case (participantsLength >= 12 && participantsLength < 20):
                numPerDivision = 4;
                break;
            case (participantsLength >= 20):
                numPerDivision = 8;
                break;
            default:
                numPerDivision = 2;
                break;
        }

        return numPerDivision;
    }

    

    this.received = function (event, ui) {
        //find bucket index based on ui.sender.attr(bucketid)
        //find participant index in participants from that bucket 
        //add it to participants
        //remove it from that bucket's participants

        /*
        var foundBucketIndex = indexOf(buckets, ui.sender.attr('bucketid'), bucketMatchesId);
        var foundParticipantIndex = indexOf(buckets[foundBucketIndex].participants, ui.item.attr('participantid'), participantMatchesId);
        that.participants.push(buckets[foundBucketIndex].participants[foundParticipantIndex]);
        buckets[foundBucketIndex].participants.splice(foundParticipantIndex, 1);
        that.participants.sort(participantComparison);
        */
        //alert(foundBucketIndex + " - " + buckets[foundBucketIndex].participants.length);
        
        that.participants.push(movedParticipant);
        that.participants.sort(participantComparison);
    }

    this.removed = function (event, ui) {
        var participantIdx = indexOf(that.participants, ui.item.attr('participantid'), participantMatchesId);
        movedParticipant = that.participants[participantIdx];
        that.participants.splice(participantIdx, 1);
    }

    this.getMinHeight = function () {
        return getCompleteHeight(participants[0].HeightFeet, participants[0].HeightInches);
    }

    this.getMaxHeight = function () {
        return getCompleteHeight(participants[participants.length - 1].HeightFeet, participants[participants.length - 1].HeightInches);
    }

    this.getMaxDifference = function () {
        return this.getMaxHeight() - this.getMinHeight();
    }

    this.sortParticipants = function () {
        this.participants.sort(participantComparison);
        $("#sortable" + this.id).empty();
        for (var i = 0; i < this.participants.length; i++) {
            var participantItem = this.getParticipantHtml(this.participants[i]);
            $("#sortable" + this.id).append(participantItem);
        }
    }
}

function DefaultBucket(participants, division, id) {
    this.prototype = Bucket;
    Bucket.call(this, participants, division, id);
    this.participants.sort(participantComparison);
    globalMinHeight = this.getMinHeight();
    var parentBucket = this.prototype;

    this.heading = "Original";
    this.getUtilLinks = function () {
        return "<a href='#' onclick='splitBucket(" + this.id + ");'>Distribute</a> <a href='#' onclick='refreshParticipants();'>Update</a> <a href='#' onclick='sortBucket(" + this.id + ");'>Sort</a>";
    };
}

//DOM's bucket ID
//bucket ID
//controller's activeBucket index
