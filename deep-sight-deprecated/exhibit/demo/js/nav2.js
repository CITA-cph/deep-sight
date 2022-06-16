
var videoLength = 0;
var videoTime = 0;
var prevTime = 0;
var startX = 0;
var isMoving = false;

var accel = 0;
var accelAmount = 0.01;
var bounceAmount = 0.9;
var targetTime = 0;


function onMouseDown(e){
	isMoving = true;
	prevTime = targetTime;
	startX = e.clientX;
}
function onMouseUp(e){
	isMoving = false;
}

function onMouseMove(e){
	if (isMoving)
	{
		//videoTime = prevTime + ((startX - e.clientX )  / $(window).width()) * videoLength;
		targetTime = prevTime + ((startX - e.clientX )  / $(window).width()) * videoLength;
		targetTime = Math.min(Math.max(targetTime, 0), videoLength);
	}
}

$(document).ready(function(){

	var video = $('#video').get(0);

	video.onloadedmetadata = function() {

		videoLength = this.duration;
		this.pause();

		var renderLoop = function(){
		  requestAnimationFrame( function(){

			videoTime += (targetTime - videoTime) * accelAmount;
			video.currentTime = videoTime;
			//video.pause();

		    document.getElementById("title").innerHTML = videoTime.toFixed(2);
		    //console.log(video.currentTime);


		    renderLoop();

		    //console.log(videoTime);
		  });
		};
		renderLoop();
	}

})
.mousedown(function(e){onMouseDown(e);})
.mouseup(function(e){onMouseUp(e);})
.mousemove(function(e){onMouseMove(e);})

$(document).bind('touchmove', onMouseMove);
$(document).bind('touchstart', onMouseDown);
$(document).bind('touchend', onMouseUp);

