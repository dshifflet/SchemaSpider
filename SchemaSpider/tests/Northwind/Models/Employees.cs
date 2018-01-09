using System;
using System.Collections.Generic;

namespace Northwind.Models
{
	//Model for NORTHWIND.EMPLOYEES (0 rows)
	public class Employees
	{
		public virtual int Id { get; set; } //EMPLOYEE_ID
		public virtual string Lastname { get; set; } //LASTNAME
		public virtual string Firstname { get; set; } //FIRSTNAME
		public virtual string Title { get; set; } //TITLE
		public virtual string TitleOfCourtesy { get; set; } //TITLE_OF_COURTESY
		public virtual DateTime? Birthdate { get; set; } //BIRTHDATE
		public virtual DateTime? Hiredate { get; set; } //HIREDATE
		public virtual string Address { get; set; } //ADDRESS
		public virtual string City { get; set; } //CITY
		public virtual string Region { get; set; } //REGION
		public virtual string PostalCode { get; set; } //POSTAL_CODE
		public virtual string Country { get; set; } //COUNTRY
		public virtual string HomePhone { get; set; } //HOME_PHONE
		public virtual string Extension { get; set; } //EXTENSION
		public virtual string Photo { get; set; } //PHOTO
		public virtual string Notes { get; set; } //NOTES
		public virtual int? ReportsTo { get; set; } //REPORTS_TO

		//CHILDREN
		public virtual IList<Orders> Orders { get; set; } //EMPLOYEE_ID [1:*] ORDERS.EMPLOYEE_ID
	}
}
