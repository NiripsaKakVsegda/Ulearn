using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace uLearn.Web.Core.Models;

public class SetNewPasswordModel
{
	[HiddenInput]
	[Required]
	public string RequestId { get; set; }

	[Required(ErrorMessage = "{0} обязателен")]
	[StringLength(100, ErrorMessage = "{0} не может быть короче {2} символов", MinimumLength = 6)]
	[DataType(DataType.Password)]
	[Display(Name = "Новый пароль")]
	public string NewPassword { get; set; }

	[DataType(DataType.Password)]
	[Display(Name = "Ещё раз")]
	[System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Пароли отличаются")]
	public string ConfirmPassword { get; set; }

	public string[]? Errors { get; set; }
}

public class RestorePasswordModel
{
	public RestorePasswordModel()
	{
		Messages = new List<Message>();
	}

	public string UserName { get; set; }
	public List<Message> Messages { get; set; }
}

public class Message
{
	public Message(string text, bool isError = true)
	{
		Text = text;
		IsError = isError;
	}

	public string Text { get; set; }
	public bool IsError { get; set; }
}