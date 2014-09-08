﻿using System;
using uLearn;
using uLearn.CSharp;

namespace OOP.Slides.U01_DataTypes_And_Algorithms
{
	[Slide("Шаблон \"Неизменяемый класс\"", "{80E68C49-0B6C-40EC-8F87-AC8D445F0DA6}")]
	public class S020_ImmutableClass
	{
		/*
		Классы, представляющие тип данных почти всегда удобно делать неизменяемыми.
		То есть такими, чтобы после создания объекта этого класса, этот объект уже никак нельзя было изменить.

		Как вы знаете, такие системные классы как `DateTime`, `String`, `TimeSpan` являются неизменяемыми.

		Зачем же нужны неизменяемые классы? Одно из главных преимуществ в том, что они делают программы значительно проще в понимании.
		
		Если у вас оказалась ссылка на неизменяемы объект, 
		вы можете быть уверены, что никто извне его не изменит. А начит рассуждать о корректности становится чуточку проще.

		Класс вектора также удобно оформить в виде неизменяемого класса. 
		Для этого ни один метод-операция не должен менять объект. Вместо этого каждый метод-операция должен возвращать **новый** вектор в качестве результата применения операции.

		Например, использованный на прошлом слайде метод вычитания векторов может быть реализован так. 
		Обратите внимание на закрытые (private) сеттеры свойств X и Y.
		*/

		public class Vector
		{
			public Vector(double x, double y)
			{
				X = x;
				Y = y;
			}

			public double X { get; private set; }
			public double Y { get; private set; }

			public Vector Subtract(Vector v)
			{
				return new Vector(X - v.X, Y - v.Y);
			}

			[Exercise]
			public Vector Normalize()
			{
				var len = Math.Sqrt(X*X + Y*Y);
				return new Vector(X / len, Y/len);
			}

			[HideOnSlide]
			public override string ToString()
			{
				return string.Format("({0}, {1})", X, Y);
			}
		}
		/*
		### Вывод

		Всегда, когда это возможно, и не приводит к значительным проблемам с производительностью, старайтесь делать ваши классы неизменяемыми.

		*/

	}
}