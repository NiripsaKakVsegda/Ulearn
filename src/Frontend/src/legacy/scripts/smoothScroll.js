function smoothScroll($object, callback, $link) {
	const needScroll = $link ? $link.data('scroll') !== false : true;
	if($object.length) {
		const scrollTo = function () {
			if(!needScroll)
				return;
			$('html').animate({
					scrollTop: $object.first().offset().top - $('.header').height()
				},
				{
					duration: 1000,
					complete: callback
				});
		};
		if($link && $link.data('toggle')) {
			if(!$object.is(':visible')) {
				$object.data('originalClasses', $object.attr('class'));
				$object.removeClass('hidden').removeClass('hidden-xs').removeClass('hidden-xs-inline');
				$object.fadeIn(200, 'linear');
				setTimeout(scrollTo, 200);
			} else {
				$object.attr('class', $object.data('originalClasses'));
			}
		} else if($object.is(':visible'))
			scrollTo();
		return true;
	}
	return false;
}

export default function () {
	$('a[href*="#"]:not([href="#"]):not(.no-smooth-scrolling)').click(function () {
		if(location.pathname.replace(/^\//, '') === this.pathname.replace(/^\//, '') && location.hostname === this.hostname) {
			let $target = $(this.hash);
			if(!$target.length) {
				$target = $('.' + this.hash.slice(1));
				if(!$target.length)
					$target = $('[name=' + this.hash.slice(1) + ']');
			}
			return !smoothScroll($target, null, $(this));
		}
	});
}
