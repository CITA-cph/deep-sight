function scrollVideo() {
  var video = $('#video').get(0),
    videoLength = video.duration,
    scrollPosition = $(document).scrollTop();
  
  video.currentTime = (scrollPosition / ($(document).height() - $(window).height())) * videoLength;
}

var speed = 10;
var videoTime = 0;
var prevTime = 0;
var startX = 0;
var videoLength = 0;
var isMoving = false;

var accel = 0;
var accelAmount = 0.01;
var bounceAmount = 0.9;
var targetTime = 0;


$(document).ready(function(){

	var video = $('#video').get(0);
	video.onloadedmetadata = function() {

		videoLength = this.duration;

		console.log(videoTime);
		console.log(video.duration);

		this.on("touchstart mousedown", function(e){

			e.preventDefault();
			isMoving = true;
			prevTime = targetTime;
			startX = e.clientX;
		});

		this.on("touchend mouseup", function(e){
			isMoving = false;

		});

		this.on("touchmove mousemove", function(e){
			if (isMoving)
			{
				//videoTime = prevTime + ((startX - e.clientX )  / $(window).width()) * videoLength;
				targetTime = prevTime + ((startX - e.clientX )  / $(window).width()) * videoLength;
				targetTime = Math.min(Math.max(targetTime, 0), videoLength);
			}
		});

		// setInterval(function(){
		requestAnimationFrame(function(){
			//Accelerate towards the target:
			accel += (targetTime - video.currentTime) * accelAmount;

			//clamp the acceleration so that it doesnt go too fast
			if (accel > 5) accel = 5;
			if (accel < -5) accel = -5;

			if(speed > 0)
			{

				//videoTime = (videoTime + accel) * (bounceAmount) + (targetTime * (1-bounceAmount));
				videoTime += (targetTime - videoTime) * accelAmount;

				//var n = Math.floor(videoTime / videoLength);
				//videoTime = videoTime - videoLength * n;
				if(videoTime > videoLength)
					videoTime = videoLength;
				else if(videoTime < 0)
					videoTime = 0;

				video.currentTime = videoTime;
				//console.log(videoTime);

				//videoTime = videoTime + (speed * 0.01);
				//speed = speed - 0.01;
			}
			if (speed < 1)
				speed = 1;
		}, 20)
	}

})
// .mousedown(function(e){
// 	isMoving = true;
// 	prevTime = targetTime;
// 	startX = e.clientX;
// })
// .mouseup(function(e){
// 	isMoving = false;
// })
// .mousemove(function(e){
// 	//console.log(e.clientX / $(window).width());
// 	if (isMoving)
// 	{
// 		//videoTime = prevTime + ((startX - e.clientX )  / $(window).width()) * videoLength;
// 		targetTime = prevTime + ((startX - e.clientX )  / $(window).width()) * videoLength;
// 		targetTime = Math.min(Math.max(targetTime, 0), videoLength);
// 	}
// })



// $(window).scroll(function(e) {
//   scrollVideo();
// });