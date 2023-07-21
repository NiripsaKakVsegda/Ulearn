using System;
using System.Runtime.Serialization;
using Database.Models;
using JetBrains.Annotations;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Models.Responses;

[DataContract]
public class CourseAccessResponse
{
	[DataMember]
	public int Id { get; set; }

	[DataMember]
	public string CourseId { get; set; }

	[DataMember]
	public ShortUserInfo User { get; set; }

	[DataMember]
	public ShortUserInfo GrantedBy { get; set; }

	[DataMember]
	public CourseAccessType AccessType { get; set; }

	[DataMember]
	public DateTime GrantTime { get; set; }

	[DataMember]
	[CanBeNull]
	public DateTime? ExpiresOn { get; set; }

	[DataMember]
	public string Comment { get; set; }
}

[DataContract]
public class ShortCourseAccessResponse
{
	[DataMember]
	public int Id { get; set; }

	[DataMember]
	public string CourseId { get; set; }

	[DataMember]
	public ShortUserInfo GrantedBy { get; set; }

	[DataMember]
	public CourseAccessType AccessType { get; set; }

	[DataMember]
	public DateTime GrantTime { get; set; }

	[DataMember]
	[CanBeNull]
	public DateTime? ExpiresOn { get; set; }
}