import { guides as defaultGuides } from '../utils/consts';
import { RateTypes } from "src/consts/rateTypes";
import {
	CourseFlashcard,
	FlashcardModerationStatus,
	FlashcardType,
	QuestionWithAnswer,
	UnitFlashcards,
	UserGeneratedFlashcard
} from "src/models/flashcards";
import { InfoByUnit } from "src/models/course";
import { mockFunc } from "../../../utils/storyMock";
import { FlashcardsActions } from "../Flashcards/Flashcards.types";

const flashcards: UnitFlashcards[] = [
	{
		unitId: 'e1beb629-6f24-279a-3040-cf111f91e764',
		unitTitle: 'Первое знакомство с C#',
		unlocked: false,
		flashcards: [
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "914883a9-d782-46af-8ffc-165a5a421866",
				question: "<p>Что такое тип переменной?</p>\n",
				answer: "<p>Тип определяет множество допустимых значений переменной, множество допустимых операций с этим значением, а также формат хранения значения в памяти.</p>\n",
				rate: RateTypes[RateTypes["notRated"]],
				courseId: 'basicprogramming2',
				unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
				lastRateIndex: 198,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "c57945a1-2877-4d92-b796-d247b9d0afbb",
				question: "<p>Как преобразовать строковое представление числа в <code>double</code>?</p>\n",
				answer: "<p><code>double.Parse</code> или\r\n<code>Convert.ToDouble</code> или\r\n<code>double.TryParse</code></p>\n",
				rate: RateTypes[RateTypes["rate1"]],
				courseId: 'basicprogramming2',
				unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
				lastRateIndex: 199,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "e1e61f5b-d931-453b-8ee5-b488f8ad13e4",
				question: "<p>Как объявить метод в C#?</p>\n",
				answer: "<p>Указать возвращаемое значение, имя метода, список аргументов в скобках с указанием их типов.</p>\n<p>Если метод статический, то начать объявление нужно с ключевого слова <code>static</code>.\r\nНапример, так:</p>\n<textarea class='code code-sample' data-lang='csharp'>static bool Method(int arg1, double arg2)</textarea>\n",
				rate: RateTypes[RateTypes["rate1"]],
				courseId: 'basicprogramming2',
				unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
				lastRateIndex: 100,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "7fd3939b-1bf6-424c-b211-685da14c1d46",
				question: "<p>Что такое <code>var</code>? Корректна ли следующая запись?</p>\n<textarea class='code code-sample' data-lang='csharp'>var a;</textarea>\n",
				answer: "<p>Ключевое слово, заменяющее тип при объявлении переменной.\r\nЗаставляет компилятор определить наиболее подходящий тип на основании анализа выражения справа от знака\r\nприсваивания.</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">var a = new int[5];</textarea><p>полностью эквивалентно</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">int[] a = new int [5];</textarea><p>Если справа нет значения, то компилятор не сможет вывести тип var и выдаст ошибку компиляции.</p>\n",
				rate: RateTypes[RateTypes["rate1"]],
				courseId: 'basicprogramming2',
				unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
				lastRateIndex: 200,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "5e8be18d-1cfb-43c6-b7c5-6a4eddc4a369",
				question: "<p>Как прочитать целое число из консоли?</p>\n",
				answer: "<p><code>Console.ReadLine</code>, а затем <code>int.Parse</code>.\r\nИли посимвольное чтение с помощью <code>Console.Read</code>, а затем <code>int.Parse</code></p>\n",
				rate: RateTypes[RateTypes["rate1"]],
				courseId: 'basicprogramming2',
				unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
				lastRateIndex: 201,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "0c22338a-13d8-4854-bfc4-9e7afd52fa95",
				question: "<p>Пусть <code>a</code> и <code>b</code> имеют тип <code>double</code>. Какие значения может принимать следующее выражение?</p>\n<p><code>(a + b)*(a + b) == a*a + 2*a*b + b*b</code></p>\n",
				answer: "<p>И <code>true</code> и <code>false</code>.</p>\n<p><code>false</code>, если в результате арифметических операций над типом <code>double</code> накопилась\r\nпогрешность.</p>\n<p>Погрешность появляется из-за того, что <code>double</code> может принимать лишь ограниченное количество значений,\r\nа действительных чисел бесконечно много. Поэтому из-за порядка вычисления операций результат может\r\nотличаться.</p>\n",
				rate: RateTypes[RateTypes["rate1"]],
				courseId: 'basicprogramming2',
				unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
				lastRateIndex: 105,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "97f894c7-4217-48d9-974d-bf88a6ab1d6d",
				question: "<p>Когда следует выделять метод?</p>\n",
				answer: "<ol>\n<li>Если метод слишком длинный, его стоит разбить на логические этапы, выделяя их в методы.</li>\n<li>Когда одна последовательность инструкций повторяется несколько раз, с небольшими вариациями стоить\r\nвыделить ее в метод <em>(DRY)</em>.</li>\n<li>Если метод делает несколько принципиально различных дел одновременно, стоит их разнести по разным методам.</li>\n</ol>\n",
				rate: RateTypes[RateTypes["rate1"]],
				courseId: 'basicprogramming2',
				unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
				lastRateIndex: 90
			},
		]
	},
	{
		unitId: '6c13729e-817b-a437-b9d3-275c01f8f4a8',
		unitTitle: 'Ошибки',
		unlocked: true,
		flashcards: [
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "61de4e20-6327-4177-b13c-4dd6011b11ba",
				question: "<p>Какие типы ошибок могут возникать в процессе разработки программы?</p>\n",
				answer: "<p>В курсе явно указывались ошибки компиляции, времени выполнения, стилистические.\r\nК этому можно добавить ошибки в алгоритмах и ошибки проектирования.</p>\n",
				rate: RateTypes["rate2"],
				courseId: 'basicprogramming2',
				unitId: "6c13729e-817b-a437-b9d3-275c01f8f4a8",
				lastRateIndex: 204,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "aadabf1c-2736-4d1a-ab90-c3ea6fb57863",
				question: "<p>Как запустить программу под отладчиком в Visual Studio?</p>\n",
				answer: "<p>Клавишей F5. Кроме того можно подключиться к уже запущенному процессу через Debug/Attach Process</p>\n",
				rate: RateTypes["rate2"],
				courseId: 'basicprogramming2',
				unitId: "6c13729e-817b-a437-b9d3-275c01f8f4a8",
				lastRateIndex: 203,
			},
		]
	},
	{
		unitId: '148775ee-9ffa-8932-64d6-d64380484169',
		unitTitle: 'Ветвления',
		unlocked: false,
		flashcards: [
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "28f3561e-9372-492a-875c-4d4b03b4a305",
				question: "<p>Для чего применяется слово <code>default</code> в выражении <code>switch</code>?</p>\n",
				answer: "<p>Чтобы указать инструкции, которые надо выполнить, если ни один из <code>case</code> не подошел.</p>\n",
				rate: RateTypes["rate5"],
				courseId: 'basicprogramming2',
				unitId: "148775ee-9ffa-8932-64d6-d64380484169",
				lastRateIndex: 111,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "8469b0ef-c676-450e-aac7-89b34ef18785",
				question: "<p>Какие есть возможности сократить этот код:</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">if (a)\r\n    return 25;\r\nelse\r\n    return 0;</textarea>",
				answer: "<p>Убрать <code>else</code> или переписать с помощью тернарного оператора в одно выражение</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">return a ? 25 : 0;</textarea>",
				rate: RateTypes["rate5"],
				courseId: 'basicprogramming2',
				unitId: "148775ee-9ffa-8932-64d6-d64380484169",
				lastRateIndex: 60,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "bc7a106e-51e4-4d13-9b05-c16359966e8a",
				question: "<p>Что может находиться внутри круглых скобок выражения <code>if</code></p>\n",
				answer: "<p>Любое выражение с типом <code>bool</code></p>\n",
				rate: RateTypes["rate5"],
				courseId: 'basicprogramming2',
				unitId: "148775ee-9ffa-8932-64d6-d64380484169",
				lastRateIndex: 116,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "BAB44484-2045-446B-BF8B-0B8C8176D07E",
				question: "<p>Как выбрать между конструкциями <code>if</code>, <code>? :</code> и <code>switch</code>?</p>\n",
				answer: "<ol>\n<li>Если в зависимости от условия нужно выбрать одно из двух значений — использовать тернарный оператор <code>condition ? value1 : value2</code>.</li>\n<li>Если нужно выбрать один из многих вариантов в зависимости от некоторого значения — подойдёт <code>switch</code>.</li>\n<li>В остальных случаях нужно использовать <code>if</code>.</li>\n</ol>\n",
				rate: RateTypes["rate5"],
				courseId: 'basicprogramming2',
				unitId: "148775ee-9ffa-8932-64d6-d64380484169",
				lastRateIndex: 63,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "8D747C8F-10C7-420E-98B0-64C7B9889F14",
				question: "<p>Чем отличаются операторы <code>||</code> и <code>|</code>, а также <code>&amp;&amp;</code> и <code>&amp;</code>?</p>\n",
				answer: "<ol>\n<li>«Удвоенные» операторы не вычисляют второй аргумент, если результат полностью определяется первым аргументом.</li>\n<li>«Одинарные» вычисляют оба своих аргумента. Также они работают не только с типом <code>bool</code>, но и с числовыми типами данных, выполняя побитовые логические операции.</li>\n</ol>\n",
				rate: RateTypes["rate5"],
				courseId: 'basicprogramming2',
				unitId: "148775ee-9ffa-8932-64d6-d64380484169",
				lastRateIndex: 67,
			}
		]
	},
	{
		unitId: 'd083f956-8fa4-6024-04da-cfc8e17bd9db',
		unitTitle: 'Циклы',
		unlocked: false,
		flashcards: [
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "8faa79ff-b453-45b2-8913-b6584c4c21ec",
				question: "<p>Что делает <code>continue</code> в цикле?</p>\n",
				answer: "<p>Переходит к следующей итерации текущего цикла.</p>\n",
				rate: RateTypes["rate2"],
				courseId: 'basicprogramming2',
				unitId: "d083f956-8fa4-6024-04da-cfc8e17bd9db",
				lastRateIndex: 124,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "acf087b1-8c92-4cba-8a30-90e5b0c01be2",
				question: "<p>Как сделать бесконечный цикл на <code>for</code>?</p>\n",
				answer: "\n<textarea class=\"code code-sample\" data-lang=\"csharp\">for ( ; ; )\r\n{ ... }</textarea>",
				rate: RateTypes["rate2"],
				courseId: 'basicprogramming2',
				unitId: "d083f956-8fa4-6024-04da-cfc8e17bd9db",
				lastRateIndex: 125,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "6FB4525E-10B4-4559-8C66-DEC3A624280B",
				question: "<p>Когда стоит использовать цикл <code>for</code>, а не <code>while</code>?</p>\n",
				answer: "<ol>\n<li>Заранее известно сколько раз нужно повторить действие — используйте <strong>for</strong>.</li>\n<li>Есть переменная, изменяемая каждую итерацию и у нее есть понятный смысл — используйте <strong>for</strong>.</li>\n</ol>\n",
				rate: RateTypes["rate2"],
				courseId: 'basicprogramming2',
				unitId: "d083f956-8fa4-6024-04da-cfc8e17bd9db",
				lastRateIndex: 126,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "3A802D29-1FC6-40EC-A26A-7AB599069A67",
				question: "<p>Как можно досрочно прервать цикл <code>for</code>?</p>\n",
				answer: "<p>Оператор <code>break</code> прерывает выполнение любого цикла, в котором он вызывается.</p>\n",
				rate: RateTypes["rate2"],
				courseId: 'basicprogramming2',
				unitId: "d083f956-8fa4-6024-04da-cfc8e17bd9db",
				lastRateIndex: 127,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "0F8E776D-72E5-4CB3-9350-2EB0405D9A7E",
				question: "<p>Можно ли с помощью <code>while</code> сэмулировать цикл <code>for</code>? А наоборот?</p>\n",
				answer: "<p>Можно и в ту и в другую сторону, вот так:</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">// for → while:\r\nfor (int i=0; i<10; i++)\r\n  Do(i);\r\n// ↓\r\nint i=0;\r\nwhile (i<10) \r\n  Do(i++);\r\n          \r\n// while → for:\r\nwhile (Condition())\r\n  Do();\r\n// ↓\r\nfor(;Condition();)\r\n  Do(i);</textarea>",
				rate: RateTypes["rate2"],
				courseId: 'basicprogramming2',
				unitId: "d083f956-8fa4-6024-04da-cfc8e17bd9db",
				lastRateIndex: 123,
			}
		]
	},
	{
		unitId: 'c777829b-7226-9049-ddf9-895234334f3f',
		unitTitle: 'Массивы',
		unlocked: false,
		flashcards: [
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "1e0ab045-ea16-405a-9aa0-5e8e8ba77fef",
				question: "<p>Как получить последний элемент в массиве?</p>\n",
				answer: "<p>Использовать длину массива <code>xs[xs.Length - 1]</code></p>\n",
				rate: RateTypes["rate2"],
				courseId: 'basicprogramming2',
				unitId: "c777829b-7226-9049-ddf9-895234334f3f",
				lastRateIndex: 162,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "ff7e1847-c5e2-4871-8aed-da93edb0332f",
				question: "<p>Чему равно выражение</p>\n<textarea class='code code-sample' data-lang='csharp'>new int[] { 1, 2, 3 } == new int[] { 1, 2, 3 }</textarea>\n",
				answer: "<p><code>false</code>. Массивы сравниваются по ссылкам. Здесь массивы — это 2 разных объекта, поэтому ссылки не\r\n                будут равны.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate2"],
				unitId: "c777829b-7226-9049-ddf9-895234334f3f",
				lastRateIndex: 154,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "35a9048a-41c2-45de-9be4-5eb86812a6bd",
				question: "<p>Какое значение получить быстрее в массиве из 1000 элементов: <code>values[0]</code> или <code>values[999]</code>?</p>\n",
				answer: "<p>Одинаково. Индекс всего лишь означает сдвиг адреса, из которого будет прочитано значение.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate3"],
				unitId: "c777829b-7226-9049-ddf9-895234334f3f",
				lastRateIndex: 155,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "dad7ad24-7bee-4b52-a925-369c60e5a019",
				question: "<p>Где хранится строка? В куче или на стеке?</p>\n",
				answer: "<p>В куче. Как правило все значения, которые могут занимать значительный объем данных (строки,\r\n                массивы) хранятся в куче, а не на стеке.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate4"],
				unitId: "c777829b-7226-9049-ddf9-895234334f3f",
				lastRateIndex: 156,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "FAF33DE8-1A32-4BBE-A0F0-8C4A44F5B51F",
				question: "<p>В чем отличие массива массивов от двумерного массива?</p>\n",
				answer: "<ol>\n<li>Каждый массив в массиве массивов нужно отдельно инициализировать.</li>\n<li>Каждый массив в массиве массивов может быть своей длины.</li>\n<li>Все элементы двумерного массива располагаются в памяти подряд.</li>\n</ol>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate4"],
				unitId: "c777829b-7226-9049-ddf9-895234334f3f",
				lastRateIndex: 157,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "53482D41-D6DC-4997-98C0-742CF3FFD8C6",
				question: "<p>Какие операции допустимы с массивами?</p>\n",
				answer: "<ol>\n<li>Узнать длину массива.</li>\n<li>Прочитать элемент массива по индексу.</li>\n<li>Записать элемент в ячейку с заданным индексом.</li>\n<li>Невозможно: изменить длину массива, добавить новый элемент в массив, удалить элемент из массива.</li>\n</ol>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate4"],
				unitId: "c777829b-7226-9049-ddf9-895234334f3f",
				lastRateIndex: 158,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "B63DA22A-A87F-4ED7-93F7-3C16479A87C0",
				question: "<p>Что делать, если необходимо добавить в массив ещё один элемент?</p>\n",
				answer: "<ol>\n<li>Придётся создать новый массив большего размера и скопировать в него все элементы исходного массива и новый элемент.</li>\n<li>Либо можно сразу создать массив большего размера, чем нужно, и в отдельной переменной хранить, сколько ячеек массива занято значениями. Но как только «запасные» ячейки закончатся, все равно придётся пересоздавать массив как в п.1.</li>\n</ol>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate4"],
				unitId: "c777829b-7226-9049-ddf9-895234334f3f",
				lastRateIndex: 159,
			}
		]
	},
	{
		unitId: 'c8da504b-817b-4c22-b2be-f05c9345cebc',
		unitTitle: 'Коллекции, строки, файлы',
		unlocked: false,
		flashcards: [
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "c8da504b-817b-4c22-b2be-f05c9345cebc",
				question: "<p>В чем ошибка? Как исправить?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">File.ReadAllLines(\"C:\\doc.txt\")</textarea>",
				answer: "<p>Слеши считаются спец-символами экранирования. Либо их нужно экранировать (удвоить)\r\n<code>&quot;C:\\\\\\\\doc.txt&quot;</code>, либо использовать &quot;дословный&quot; формат строки\r\n<code>@&quot;C:\\doc.txt&quot;</code></p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
				lastRateIndex: 169,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "36e47c38-af0d-41c7-8240-7da24e4c3f13",
				question: "<p>Чем отличается массив от списка (<code>List</code>)?</p>\n",
				answer: "<p>У массива фиксирована длина — она задаётся при создании массива и её нельзя поменять. Список увеличивает\r\nсвою длину по мере необходимости.\r\nВ отличие от массива у него есть метод <code>Add(item)</code></p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
				lastRateIndex: 170,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "17ece429-52ee-4509-a26e-c7d3a4bcafe2",
				question: "<p>Для чего нужно экранирование символов в строке? Какие символы нужно экранировать?</p>\n",
				answer: "<p>Как минимум нужно экранировать кавычки, переводы строк и сами слеши. Потому что без экранирования\r\n                они обозначают что-то другое: кавычка заканчивает строку, перевод строк не может содержаться внутри\r\n                строки, а слеши используются для экранирования других символов.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate3"],
				unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
				lastRateIndex: 165,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "5a8fadc8-bad4-482b-9f24-fef0df4aec72",
				question: "<p>В каких случаях нужно использовать <code>StringBuilder</code>?</p>\n",
				answer: "<p>Когда надо собрать длинную строчку из большого числа маленьких. Например, из массива строк.\r\n                Другие способы эффективно собрать строку из нескольких маленьких: <code>string.Concat</code>, <code>string.Join</code> и\r\n                <code>string.Format</code>.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate4"],
				unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
				lastRateIndex: 166,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "e2a7effc-0b16-4194-bc2c-05ba77be8ae0",
				question: "<p>Что общего у <code>StringBuilder</code> и <code>List</code>?</p>\n",
				answer: "<p>Оба выделяют память с запасом и динамически увеличивают свой размер по мере добавления новых\r\nданных. Оба предоставляют методы для добавления, удаления и модификации содержимого.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
				lastRateIndex: 167,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "15abf901-49d1-43b1-bcec-ba7dd1605d8d",
				question: "<p>В чем ошибка?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">string a;\r\nvar parts = a.Split(\" \");</textarea>",
				answer: "<p>Переменная <code>a</code> не инициализирована. \r\n                Будет ошибка компиляции, указывающая на попытку использовать неинициализированную переменную.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
				lastRateIndex: 168,
			},
		]
	},
	{
		unitId: 'fab42d9c-db92-daec-e883-7d5424eb6c13',
		unitTitle: 'Сложность алгоритмов',
		unlocked: false,
		flashcards: [
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "9acda6fa-5e8e-4019-b060-2e2a5c88b8f1",
				question: "<p>Пусть <code>xs</code> — массив целых чисел.\r\n                Чему равна сложность поиска максимального элемента <code>Max(xs)</code>?\r\n                При каких условиях сложность может быть константной?</p>\n",
				answer: "<p>Сложность линейная, потому что придется перебрать все элементы массива. \r\nНо если массив отсортирован, то найти максимум можно за <span class='tex'>O(1)</span> — просто взять последний или первый элемент \r\nв зависимости от порядка сортировки.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
				lastRateIndex: 117,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "3494eed3-d195-4563-bff3-8da88d4d39d3",
				question: "<p>Дайте определение временной сложности алгоритма</p>\n",
				answer: "<p>Это функция, которая размеру входных данных ставит в соответствие точную верхнюю грань количества\r\n                элементарных операций, которое требуется алгоритму на входах заданного размера.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
				lastRateIndex: 113,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "a04fac9a-2176-4ead-be2a-6620b1569d8a",
				question: "<p>Дайте определение ёмкостной сложности алгоритма</p>\n",
				answer: "<p>Это функция, которая размеру входных данных ставит в соответствие точную верхнюю грань количества\r\n                дополнительной памяти, которое требуется алгоритму на входах заданного размера.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
				lastRateIndex: 112,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "cf1168f2-9487-4ee9-a91c-0cbc6ccbf1d8",
				question: "<p>Зачем нужна <span class='tex'>O</span>-нотация для описания временной сложности алгоритма?</p>\n",
				answer: "<p>&quot;Количество элементарных операций&quot; в определении сложности в реальности очень условное\r\n                понятие. Процессоры устроены сложно и даже одна и та же операция может иметь разную стоимость в\r\n                зависимости от ситуации. Поэтому точное значение сложности не имеет смысла, а проще работать с оценкой\r\n                сложности.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
				lastRateIndex: 45,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "5e0b7ab5-c336-4b5c-b07d-2e943599c54e",
				question: "<p>Чем <span class='tex'>\\Theta(n)</span> отличается от <span class='tex'>O(n)</span>?</p>\n",
				answer: "<p><span class='tex'>\\Theta(n)</span> — это точная оценка асимптотики, а <span class='tex'>O</span> — лишь оценка сверху.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate1"],
				unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
				lastRateIndex: 106,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "f1babb90-2f3f-49d0-a796-6e4c7d3a043e",
				question: "<p>Какова сложность добавления элемента в начало <em>List</em>?</p>\n",
				answer: "<p><span class='tex'>O(n)</span>. <em>List</em> реализован поверх массива. Для вставки нового элемента в начало, придется сдвинуть\r\n                вправо все элементы <em>List</em> на одну позицию.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
				lastRateIndex: 115,
			},
		]
	},
	{
		unitId: '40de4f88-54d6-3c23-faee-0f9de37ad824',
		unitTitle: 'Рекурсивные алгоритмы',
		unlocked: false,
		flashcards: [
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "F569E6D7-1EA0-4ADA-BB93-8E2B03F845E6",
				question: "<p>В чём идея подхода «Разделяй и властвуй» к проектированию алгоритмов.</p>\n",
				answer: "<ol>\n<li>Разбить всю задачу на несколько подзадач меньшего размера.</li>\n<li>Научиться решать задачи для задач минимального размера.</li>\n<li>Решить каждую подзадачу тем же способом.</li>\n<li>Объединить решения подзадач в решение целой задачи.</li>\n</ol>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "40de4f88-54d6-3c23-faee-0f9de37ad824",
				theorySlidesIds: ["c233ec6f-5803-48e5-a67c-b5824baa7582"],
				lastRateIndex: 174,
				theorySlides: [{
					slug: "Razdelyay_i_vlastvuy_c233ec6f-5803-48e5-a67c-b5824baa7582",
					title: "Разделяй и властвуй"
				}]
			} as CourseFlashcard,
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "71FC0E76-C79F-4E1B-8895-DB541949A1D1",
				question: "<p>Что может означать исключение StackOverflowException в рекурсивной программе?</p>\n",
				answer: "<p>Скорее всего программа вошла в бесконечную рекурсию.\r\nЭто свидетельствует об ошибке в программе: программист не предусмотрел условие выхода из рекурсии или ошибся в этом условии.</p>\n<p>Это исключение может появляться и в случае, когда бесконечной рекурсии нет, но стек вызовов переполнился.\r\nДля этого нужно, чтобы было десятки тысяч вложенных вызовов методов, что бывает редко.\r\nПоэтому на практике это исключение скорее всего обозначает именно бесконечную рекурсию.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "40de4f88-54d6-3c23-faee-0f9de37ad824",
				theorySlidesIds: ["caec41b0-3166-40c0-9ded-3941c7f0b91d"],
				lastRateIndex: 175,
				theorySlides: [{ slug: "Rekursiya_caec41b0-3166-40c0-9ded-3941c7f0b91d", title: "Рекурсия" }]
			} as CourseFlashcard,
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "8f35c426-4106-470b-b416-6a2651f02dc6",
				question: "<p>Как можно посчитать сумму элементов массива без циклов и библиотечных функций?</p>\n",
				answer: "<p>С помощью рекурсии. Сумма пустого массива — 0.\r\nСумма непустого — это первый элемент + сумма оставшейся части массива.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "40de4f88-54d6-3c23-faee-0f9de37ad824",
				lastRateIndex: 176,
			},
		]
	},
	{
		unitId: '8fe5a2fc-fe15-a2a6-87b8-74f4f36af51d',
		unitTitle: 'Наследование',
		unlocked: false,
		flashcards: [
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "E74FF50B-5151-4C48-A1B3-1206E0AB8917",
				question: "<p>Что такое тип-интерфейс в языке C#?</p>\n",
				answer: "<p>Контракт, обязательство класса содержать некоторые методы и свойства.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate4"],
				unitId: "8fe5a2fc-fe15-a2a6-87b8-74f4f36af51d",
				lastRateIndex: 109,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "2F491945-CE44-4C23-A39D-562A5747ACDB",
				question: "<p>Что такое наследование и зачем оно нужно?</p>\n",
				answer: "<p>Наследование позволяет определить дочерний класс, который использует (наследует), расширяет или изменяет возможности родительского класса. \r\nКласс, члены которого наследуются, называется базовым классом. \r\nКласс, который наследует члены базового класса, называется производным классом.\r\nНаследование воплощает идею «быть частным случаем чего-то».</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate2"],
				unitId: "8fe5a2fc-fe15-a2a6-87b8-74f4f36af51d",
				lastRateIndex: 107,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "FD016719-5E82-4718-B736-D532D6F1B180",
				question: "<p>Что такое upcast?</p>\n",
				answer: "<p>Приведение значения к родительскому типу. Объект не меняется — меняется точка зрения на него.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate3"],
				unitId: "8fe5a2fc-fe15-a2a6-87b8-74f4f36af51d",
				lastRateIndex: 108,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "234A1115-F9BF-4692-B95B-30895C72C8F8",
				question: "<p>Что такое полиморфизм и зачем он нужен?</p>\n",
				answer: "<p>Полиморфизм позволяет объектам производного класса обрабатываться как объектам базового класса или интерфейса. \r\nТаким образом, алгоритмы использующие в работе некоторый интерфейс можно применять ко всем классам-наследниками.\r\nПри этом виртуальные методы и реализация интерфейса в каждом наследнике может быть различной.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "8fe5a2fc-fe15-a2a6-87b8-74f4f36af51d",
				lastRateIndex: 110,
			},
		]
	},
	{
		unitId: '97940709-9f6d-03f4-2090-63bd08befcf1',
		unitTitle: 'Структуры',
		unlocked: true,
		flashcards: [
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "3EE93A8A-9AB4-4013-83B9-C546747535D2",
				question: "<p>В чем ошибка? Как исправить?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">struct A { public B b; }\r\nclass B { public int c; }\r\nvar a = new A();\r\nConsole.WriteLine(a.b.c)</textarea>",
				answer: "<p>В поле b находится null и поэтому будет NullReferenceException.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate4"],
				unitId: "97940709-9f6d-03f4-2090-63bd08befcf1",
				lastRateIndex: 206,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "B2190C3E-4ADF-4EFE-B111-2DAC0E46908E",
				question: "<p>В чем ошибка в коде?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">public struct Point\r\n{\r\n    public Point()\r\n    {\r\n        X = 0;\r\n        Y = 0;\r\n    }\r\n    public int X;\r\n    public int Y;\r\n}</textarea>",
				answer: "<p>У структуры нельзя переопределить конструктор по умолчанию.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate3"],
				unitId: "97940709-9f6d-03f4-2090-63bd08befcf1",
				lastRateIndex: 205,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "B98EC877-6E9E-4D4B-84B5-EC6E7BEFECE1",
				question: "<p>Что делать, если надо передать структуру в метод по ссылке?</p>\n",
				answer: "<p>Использовать <code>ref</code>.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "97940709-9f6d-03f4-2090-63bd08befcf1",
				lastRateIndex: 207,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "A08AD9D2-927D-45AC-8CC8-937967449FCC",
				question: "<p>Что такое boxing?</p>\n",
				answer: "<p>Неявное копирование структуры в кучу. Например, при upcast в object или реализуемый интерфейс.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate2"],
				unitId: "97940709-9f6d-03f4-2090-63bd08befcf1",
				lastRateIndex: 208,
			},
			{
				flashcardType: FlashcardType.CourseFlashcard,
				id: "D8179FD0-F230-4677-BF8E-7422612CF233",
				question: "<p>Пусть Point — это структура, содержащая публичные поля X, Y и метод GetLength. В каких строчках есть ошибки и почему?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">Point p;\r\np.X = 5;\r\nvar length = p.GetLength();</textarea>",
				answer: "<p>Только в третьей. Потому что нельзя использовать методы не инициализированной полностью структуры. А вот присвоить значение полю можно.</p>\n",
				courseId: 'basicprogramming2',
				rate: RateTypes["rate5"],
				unitId: "97940709-9f6d-03f4-2090-63bd08befcf1",
				lastRateIndex: 171,
			}
		]
	}
];

const infoByUnits: InfoByUnit[] = [
	{
		unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
		unitTitle: "Первое знакомство с C#",
		unlocked: false,
		cardsCount: 7,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_7e536978-d860-4cca-a690-da1032a30bd5"
	},
	{
		unitId: "6c13729e-817b-a437-b9d3-275c01f8f4a8",
		unitTitle: "Ошибки",
		unlocked: true,
		cardsCount: 2,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_a99c8ea4-1033-4394-bb87-98e146f56668"
	},
	{
		unitId: "148775ee-9ffa-8932-64d6-d64380484169",
		unitTitle: "Ветвления",
		unlocked: false,
		cardsCount: 5,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_90d771b0-f8b0-443a-945f-2eeeec96a6c4"
	},
	{
		unitId: "d083f956-8fa4-6024-04da-cfc8e17bd9db",
		unitTitle: "Циклы",
		unlocked: true,
		cardsCount: 5,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_7833adde-01f1-4912-8bbe-1c4856d222d7"
	},
	{
		unitId: "c777829b-7226-9049-ddf9-895234334f3f",
		unitTitle: "Массивы",
		unlocked: true,
		cardsCount: 7,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_a577bb95-e437-4f02-860d-6cbcb57b9ab2"
	},
	{
		unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
		unitTitle: "Коллекции, строки, файлы",
		unlocked: true,
		cardsCount: 6,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_d2ccb0f7-5f1a-42fd-9b04-90248a5ca9b4"
	},
	{
		unitId: "0299fc2c-1fdc-d85d-d654-4a6a1353f64d",
		unitTitle: "Тестирование",
		unlocked: true,
		cardsCount: 6,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_12bee77d-efaf-47b6-9e52-03038831f79c"
	},
	{
		unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
		unitTitle: "Сложность алгоритмов",
		unlocked: true,
		cardsCount: 6,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_3d69c830-046b-468c-8556-14ed845bb50f"
	},
	{
		unitId: "40de4f88-54d6-3c23-faee-0f9de37ad824",
		unitTitle: "Рекурсивные алгоритмы",
		unlocked: true,
		cardsCount: 3,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_b24a1dd7-524d-466c-9603-57381ebc33ff"
	},
	{
		unitId: "e4df2f6b-dc5d-3cc1-1467-0edcc93c211a",
		unitTitle: "Поиск и сортировка",
		unlocked: true,
		cardsCount: 6,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_8ad843d8-0bf9-4a4c-97fe-a6e02a2f842b"
	},
	{
		unitId: "b8ff29db-7416-c2f6-aa37-7e58a96ed597",
		unitTitle: "Основы ООП",
		unlocked: true,
		cardsCount: 5,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_17d78630-c5fa-4c0d-aaad-1cf4edd7e1b0"
	},
	{
		unitId: "8fe5a2fc-fe15-a2a6-87b8-74f4f36af51d",
		unitTitle: "Наследование",
		unlocked: true,
		cardsCount: 4,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_4bbee343-194e-4bb8-bf9a-f072f401e81f"
	},
	{
		unitId: "1557d92c-68e6-63d0-69ee-414354353685",
		unitTitle: "Целостность данных",
		unlocked: false,
		cardsCount: 10,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_149390cf-a8c6-4c47-b374-9004561704e5"
	},
	{
		unitId: "97940709-9f6d-03f4-2090-63bd08befcf1",
		unitTitle: "Структуры",
		unlocked: true,
		cardsCount: 5,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_47be3bfd-6df5-49ff-91d5-abfb78d85cbf"
	}];

const buildQuestionWithAnswer = (question: string, answer: string,): QuestionWithAnswer => {
	return {
		question: question,
		answer: answer,
		isRendered: true
	};
};

const longQuestion = 'Если я спрошу очень большой вопрос, то сломается ли верстка? Если да, то почему? Как это поправить? Или не стоит? как думаешь? Почему?';
const longAnswer = 'Большой ответ также может сломать все, что захочет, должно быть стоит лучше приглядываться к стилям. И так же надо написать ещё 5 слов чтобы было побольше слов.';

const questionsWithAnswers: QuestionWithAnswer[] = [
	buildQuestionWithAnswer('Почему 1 больше 2?', 'Потому что 2 меньше 1... наверное'),
	buildQuestionWithAnswer('Это так?', 'Нет, не так'),
	buildQuestionWithAnswer(longQuestion, longAnswer),
];

export const userGeneratedFlashcard: UserGeneratedFlashcard = {
	flashcardType: FlashcardType.UserFlashcard,
	id: 'D8179FD0-F230-4677-BF8E-7562612CF233',
	courseId: 'basicprogramming2',
	unitId: 'e1beb629-6f24-279a-3040-cf111f91e764',
	rate: RateTypes.notRated,
	lastRateIndex: 0,
	question: 'Вопрос *курсив* **жирный**',
	answer: 'Ответ  *курсив* **жирный** ```inlineCode```',
	moderationStatus: FlashcardModerationStatus.Approved,
	isPublished: true,
	owner: {
		id: 'userId',
		visibleName: 'user',
		lastName: '',
		firstName: '',
		avatarUrl: null
	},
	lastUpdateTimestamp: '2023-07-12T23:19:26',
	moderator: {
		id: 'moderatorId',
		visibleName: 'moderator',
		lastName: '',
		firstName: '',
		avatarUrl: null
	},
	moderationTimestamp: '2023-07-12T23:29:26'
};

export const flashcardsActions: FlashcardsActions = {
	onRemoveFlashcard: mockFunc,
	onSendFlashcardRate: mockFunc,
	onEditFlashcard: mockFunc,
	onCreateFlashcard: mockFunc,
	onApproveFlashcard: mockFunc,
	onDeclineFlashcard: mockFunc
};

const longText = `Идейные соображения высшего порядка, а также рамки и место обучения кадров обеспечивает широкому кругу (специалистов) участие в формировании соответствующий условий активизации. Разнообразный и богатый опыт начало повседневной работы по формированию позиции требуют определения и уточнения позиций, занимаемых участниками в отношении поставленных задач.
Повседневная практика показывает, что новая модель организационной деятельности требуют от нас анализа систем массового участия. Товарищи! консультация с широким активом представляет собой интересный эксперимент проверки позиций, занимаемых участниками в отношении поставленных задач. Таким образом консультация с широким активом позволяет оценить значение системы обучения кадров, соответствует насущным потребностям. Идейные соображения высшего порядка, а также постоянный количественный рост и сфера нашей активности способствует подготовки и реализации систем массового участия. Разнообразный и богатый опыт постоянное информационно-пропагандистское обеспечение нашей деятельности способствует подготовки и реализации направлений прогрессивного развития. Идейные соображения высшего порядка, а также реализация намеченных плановых заданий в значительной степени обуславливает создание направлений прогрессивного развития.`;

const guides: string[] = [...defaultGuides, longText, "short"];
export { flashcards, infoByUnits, questionsWithAnswers, guides, };
