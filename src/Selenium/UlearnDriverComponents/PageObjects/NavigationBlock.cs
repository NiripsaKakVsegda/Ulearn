﻿using OpenQA.Selenium;

namespace Selenium.UlearnDriverComponents.PageObjects
{
	public class NavigationBlock : PageObject
	{
		private readonly IWebDriver driver;
		private readonly IWebElement nextSlideButton;
		private readonly IWebElement prevSlideButton;
		private readonly IWebElement nextSolutionsButton;
		public NavigationBlock(IWebDriver driver)
		{
			this.driver = driver;
			nextSlideButton = UlearnDriver.FindElementSafely(driver, ElementsId.NextNavArrow);
			prevSlideButton = UlearnDriver.FindElementSafely(driver, ElementsId.PrevNavArrow);
			nextSolutionsButton = UlearnDriver.FindElementSafely(driver, ElementsId.NextSolutionsButton);
			CheckButtons();
		}

		private void CheckButtons()
		{
			if (nextSlideButton == null)
				throw new NotFoundException("не найдена NextSlideButton");
			if (prevSlideButton == null)
				throw new NotFoundException("не найдена PrevSlideButton");
			if (nextSolutionsButton == null)
				throw new NotFoundException("не найдена NextSolutionsButton");
		}

		public void ClickNextButton()
		{
			if (nextSlideButton.Displayed)
				nextSlideButton.Click();
			else
				nextSolutionsButton.Click();
		}

		public void ClickPrevButton()
		{
			prevSlideButton.Click();
		}

		public bool HasNextButton()
		{
			return (nextSlideButton.Displayed || nextSolutionsButton.Displayed);
		}

		public bool IsHasPrevButton()
		{
			return (prevSlideButton.Displayed);
		}

		public bool IsActiveNextButton()
		{
			return IsActive(nextSlideButton.Displayed ? nextSlideButton : nextSolutionsButton);
		}

		private bool IsActive(IWebElement button)
		{
			return UlearnDriver.HasCss(button, "block_next");
		}

	}
}
