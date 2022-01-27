import React, { createRef, RefObject, SyntheticEvent } from "react";

import ImageGallery from 'react-image-gallery';

import classNames from "classnames";

import styles from './Image.less';

import 'react-image-gallery/styles/css/image-gallery.css';

export interface Props {
	className: string;
	imageUrls: string[];
}

interface ImageInfo {
	index: number;
	error?: boolean;
	img?: HTMLImageElement;
}

interface State {
	fullscreen: boolean;
	currentImage: ImageInfo;
	anyImageLoaded: boolean;
	images: ImageInfo[];
}

interface ImageGalleryRef extends ImageGallery {
	imageGallery: RefObject<HTMLDivElement>;
}

class Image extends React.Component<Props, State> {
	private gallery: RefObject<ImageGalleryRef> = createRef();
	private wrapper: RefObject<HTMLDivElement> = createRef();

	constructor(props: Props) {
		super(props);

		const images = this.props.imageUrls.map((_, index) => {
			return { index };
		});

		this.state = {
			fullscreen: false,
			currentImage: images[0],
			anyImageLoaded: false,
			images,
		};
	}

	componentDidMount(): void {
		window.addEventListener('resize', this.resizePictures);
	}

	componentWillUnmount(): void {
		window.removeEventListener('resize', this.resizePictures);
	}

	resizePictures = (): void => {
		const { images, anyImageLoaded, fullscreen, } = this.state;

		if(anyImageLoaded) {
			this.setSizeForErroredImages(images, fullscreen);
		}
	};

	get failedImagesCount(): number {
		const { images, } = this.state;

		return images.filter(i => i.error).length;
	}

	render(): React.ReactNode {
		const { imageUrls, className, } = this.props;
		const { fullscreen, currentImage, anyImageLoaded, } = this.state;

		const wrapperClass = classNames(
			styles.wrapper,
			{ [styles.error]: currentImage.error },
			{ [styles.loading]: !anyImageLoaded },
			className
		);


		return (
			<div className={ wrapperClass } onClick={ this.onClick } ref={ this.wrapper }>
				<ImageGallery
					ref={ this.gallery }
					onImageLoad={ this.onImageLoad }
					onImageError={ this.onImageError }
					onBeforeSlide={ this.onBeforeSlide }
					additionalClass={ classNames(styles.imageWrapper, { [styles.open]: fullscreen }) }
					useBrowserFullscreen={ false }
					showBullets={ imageUrls.length !== 1 }
					showFullscreenButton={ this.shouldShowFullscreenButton(currentImage) || fullscreen }
					showPlayButton={ false }
					showThumbnails={ false }
					onScreenChange={ this.onScreenChange }
					items={ imageUrls.map(url => {
						return {
							original: url,
							originalClass: styles.img,
						};
					}) }/>
			</div>
		);
	}

	onImageLoad = (event: SyntheticEvent<HTMLImageElement, Event>): void => {
		const { anyImageLoaded, } = this.state;
		const img = event.currentTarget;

		if(!anyImageLoaded) {
			this.setState({
				anyImageLoaded: true,
			});
		}

		this.addAttributeToImage(img);
	};

	onImageError = (event: SyntheticEvent<HTMLImageElement>): void => {
		const img = event.currentTarget;
		const { imageUrls, } = this.props;

		if(this.failedImagesCount === imageUrls.length - 1) {
			this.setState({
				anyImageLoaded: true,
			});
		}

		this.addAttributeToImage(img, true);
	};

	addAttributeToImage = (img: HTMLImageElement, error?: boolean): void => {
		const { imageUrls, } = this.props;
		const { images, anyImageLoaded, } = this.state;

		const src = img.getAttribute("src");
		const index = imageUrls.findIndex(url => url === src);
		const newImages = [...images];

		newImages[index].img = img;
		newImages[index].error = error;

		if(error && anyImageLoaded) {
			this.setSizeForErroredImages(newImages);
		}

		this.setState({
			images: newImages,
		});
	};

	setSizeForErroredImages = (images: ImageInfo[], fullscreen?: boolean): void => {
		const loadedImage = images.find(({ error }) => !error);

		if(loadedImage && loadedImage.img) {
			const aspectRatio = loadedImage.img.naturalHeight / loadedImage.img.naturalWidth;
			const width = Math.min(loadedImage.img.naturalWidth, fullscreen ? window.innerWidth : this.slideWidth);
			const height = width * aspectRatio;

			for (const { img } of images.filter(({ error }) => error)) {
				if(img) {
					img.style.width = `${ width }px`;
					img.style.height = `${ height }px`;
				}
			}
		}
	};

	get slideWidth(): number {
		if(this.wrapper) {
			const slideNode = this.wrapper.current?.parentElement;
			if(slideNode) {
				const slideStyle = getComputedStyle(slideNode);
				return parseFloat(slideStyle.width) - parseFloat(slideStyle.paddingLeft) - parseFloat(
					slideStyle.paddingRight);
			}
		}
		return -1;
	}

	shouldShowFullscreenButton = ({ img, error, }: ImageInfo): boolean => {
		if(this.wrapper && img) {
			return !error && img.width >= this.slideWidth;
		}

		return false;
	};

	onScreenChange = (isFullScreen: boolean): void => {
		this.setSizeForErroredImages(this.state.images, isFullScreen);

		this.setState({
			fullscreen: isFullScreen,
		});
	};

	onBeforeSlide = (index: number): void => {
		const { images, } = this.state;
		const currentImage = images[index];

		this.setState({
			currentImage,
		});
	};

	onClick = (e: React.MouseEvent<HTMLDivElement, MouseEvent>): void => {
		if(this.gallery.current && this.gallery.current.imageGallery.current === e.target && this.state.fullscreen) { //if root component clicked (in fullscreen its background)
			this.gallery.current.exitFullScreen();
		}
	};
}

export default Image;
