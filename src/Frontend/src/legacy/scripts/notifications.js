export default function () {
	const $notificaticationsIconLink = $('.notifications__icon-link');
	const $counter = $('.notifications__counter');
	const $dropdownContent = $('.notifications__dropdown');
	let lastCountUpdateTimestamp = $notificaticationsIconLink.data('lastTimestamp');

	/* On mobile layout close user menu on notification area showing */
	const hideUserMenu = function () {
		$('.greeting-collapse-class').removeClass('in');
	};

	/* On mobile layout close notification area on user menu showing */
	$('.greeting-collapse-class').on('show.bs.collapse', function () {
		$('.notifications__mobile-dropdown').removeClass('in');
	});

	$notificaticationsIconLink.click(function () {
		const $self = $(this);
		const $dropdown = $self.closest('.dropdown');
		const isDesktopVersionOpened = $dropdown.hasClass('open');
		const isMobileVersionOpened = $('.notifications__mobile-dropdown').hasClass('in');
		if(!isDesktopVersionOpened && !isMobileVersionOpened) {
			const loadUrl = $self.data('notificationsUrl');
			$dropdownContent.html('<li class="notifications__info">Загружаю последние уведомления...</li>');
			$.get(loadUrl, function (data) {
				$dropdownContent.html(data);
				$counter.text('0').hide();
			});
		}

		hideUserMenu();
	});

	$(document).on('click', '.notifications__notification', function (e) {
		e.preventDefault();

		const linkElem = $(this).find('> *');
		const link = linkElem.data('href');

		if(link) {
			const getLocation = function (href) {
				const l = document.createElement("a");
				l.href = href;
				return l;
			};
			if(window.location.hostname !== getLocation(link).hostname)
				window.open(link, '_blank');
			else
				window.location.href = link;
		}
	});

	$(document).on('click', '.notifications__feed-switcher a', function (e) {
		e.preventDefault();
		e.stopPropagation();

		const $self = $(this);
		const feed = $self.data('feed');
		if($self.hasClass('active'))
			return;

		const updateTimestampUrl = $self.data('updateTimestampUrl');
		if(updateTimestampUrl) {
			$.post(updateTimestampUrl);
		}

		const $feeds = $('.notifications__feed');
		const $switchers = $('.notifications__feed-switcher a');
		$switchers.removeClass('active');
		$feeds.removeClass('active');

		$self.addClass('active');
		$feeds.filter('[data-feed="' + feed + '"]').addClass('active');
	});

	const updateNotificationUnreadCount = function () {
		const url = $notificaticationsIconLink.data('countUrl').replace('LAST_TIMESTAMP', lastCountUpdateTimestamp);
		$.getJSON(url, function (data) {
			if(data.status === 'ok') {
				const unreadCount = data.count;
				if(unreadCount > 0) {
					const currentCount = parseInt($counter.text());
					const newCount = Math.min(currentCount + unreadCount, 99);
					$counter.text(newCount).show();
					lastCountUpdateTimestamp = data.last_timestamp;
				}
			} else {
				alert(data.error);
			}
		});
	};

	if($('.notifications__icon-link').length > 0)
		setInterval(updateNotificationUnreadCount, 60 * 1000);
}
