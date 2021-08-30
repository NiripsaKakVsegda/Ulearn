import { guides as defaultGuides } from '../consts';

import { RateTypes } from "src/consts/rateTypes";
import { Flashcard, QuestionWithAnswer } from "src/models/flashcards";
import { InfoByUnit } from "src/models/course";

const flashcards: Flashcard[] = [
	{
		id: "914883a9-d782-46af-8ffc-165a5a421866",
		question: "<p>Что такое тип переменной?</p>\n",
		answer: "<p>Тип определяет множество допустимых значений переменной, множество допустимых операций с этим значением, а также формат хранения значения в памяти.</p>\n",
		unitTitle: "Первое знакомство с C#",
		rate: RateTypes[RateTypes["notRated"]],
		unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
		theorySlidesIds: [],
		lastRateIndex: 198,
		theorySlides: []
	}, {
		id: "c57945a1-2877-4d92-b796-d247b9d0afbb",
		question: "<p>Как преобразовать строковое представление числа в <code>double</code>?</p>\n",
		answer: "<p><code>double.Parse</code> или\r\n<code>Convert.ToDouble</code> или\r\n<code>double.TryParse</code></p>\n",
		unitTitle: "Первое знакомство с C#",
		rate: RateTypes[RateTypes["rate1"]],
		unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
		theorySlidesIds: [],
		lastRateIndex: 199,
		theorySlides: []
	}, {
		id: "e1e61f5b-d931-453b-8ee5-b488f8ad13e4",
		question: "<p>Как объявить метод в C#?</p>\n",
		answer: "<p>Указать возвращаемое значение, имя метода, список аргументов в скобках с указанием их типов.</p>\n<p>Если метод статический, то начать объявление нужно с ключевого слова <code>static</code>.\r\nНапример, так:</p>\n<textarea class='code code-sample' data-lang='csharp'>static bool Method(int arg1, double arg2)</textarea>\n",
		unitTitle: "Первое знакомство с C#",
		rate: RateTypes["rate2"],
		unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
		theorySlidesIds: [],
		lastRateIndex: 100,
		theorySlides: []
	}, {
		id: "7fd3939b-1bf6-424c-b211-685da14c1d46",
		question: "<p>Что такое <code>var</code>? Корректна ли следующая запись?</p>\n<textarea class='code code-sample' data-lang='csharp'>var a;</textarea>\n",
		answer: "<p>Ключевое слово, заменяющее тип при объявлении переменной.\r\nЗаставляет компилятор определить наиболее подходящий тип на основании анализа выражения справа от знака\r\nприсваивания.</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">var a = new int[5];</textarea><p>полностью эквивалентно</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">int[] a = new int [5];</textarea><p>Если справа нет значения, то компилятор не сможет вывести тип var и выдаст ошибку компиляции.</p>\n",
		unitTitle: "Первое знакомство с C#",
		rate: RateTypes["rate3"],
		unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
		theorySlidesIds: [],
		lastRateIndex: 200,
		theorySlides: []
	}, {
		id: "5e8be18d-1cfb-43c6-b7c5-6a4eddc4a369",
		question: "<p>Как прочитать целое число из консоли?</p>\n",
		answer: "<p><code>Console.ReadLine</code>, а затем <code>int.Parse</code>.\r\nИли посимвольное чтение с помощью <code>Console.Read</code>, а затем <code>int.Parse</code></p>\n",
		unitTitle: "Первое знакомство с C#",
		rate: RateTypes["rate4"],
		unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
		theorySlidesIds: [],
		lastRateIndex: 201,
		theorySlides: []
	}, {
		id: "0c22338a-13d8-4854-bfc4-9e7afd52fa95",
		question: "<p>Пусть <code>a</code> и <code>b</code> имеют тип <code>double</code>. Какие значения может принимать следующее выражение?</p>\n<p><code>(a + b)*(a + b) == a*a + 2*a*b + b*b</code></p>\n",
		answer: "<p>И <code>true</code> и <code>false</code>.</p>\n<p><code>false</code>, если в результате арифметических операций над типом <code>double</code> накопилась\r\nпогрешность.</p>\n<p>Погрешность появляется из-за того, что <code>double</code> может принимать лишь ограниченное количество значений,\r\nа действительных чисел бесконечно много. Поэтому из-за порядка вычисления операций результат может\r\nотличаться.</p>\n",
		unitTitle: "Первое знакомство с C#",
		rate: RateTypes["rate5"],
		unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
		theorySlidesIds: [],
		lastRateIndex: 105,
		theorySlides: []
	}, {
		id: "97f894c7-4217-48d9-974d-bf88a6ab1d6d",
		question: "<p>Когда следует выделять метод?</p>\n",
		answer: "<ol>\n<li>Если метод слишком длинный, его стоит разбить на логические этапы, выделяя их в методы.</li>\n<li>Когда одна последовательность инструкций повторяется несколько раз, с небольшими вариациями стоить\r\nвыделить ее в метод <em>(DRY)</em>.</li>\n<li>Если метод делает несколько принципиально различных дел одновременно, стоит их разнести по разным методам.</li>\n</ol>\n",
		unitTitle: "Первое знакомство с C#",
		rate: RateTypes["rate5"],
		unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
		theorySlidesIds: [],
		lastRateIndex: 90,
		theorySlides: []
	}, {
		id: "61de4e20-6327-4177-b13c-4dd6011b11ba",
		question: "<p>Какие типы ошибок могут возникать в процессе разработки программы?</p>\n",
		answer: "<p>В курсе явно указывались ошибки компиляции, времени выполнения, стилистические.\r\nК этому можно добавить ошибки в алгоритмах и ошибки проектирования.</p>\n",
		unitTitle: "Ошибки",
		rate: RateTypes["rate2"],
		unitId: "6c13729e-817b-a437-b9d3-275c01f8f4a8",
		theorySlidesIds: [],
		lastRateIndex: 204,
		theorySlides: []
	}, {
		id: "aadabf1c-2736-4d1a-ab90-c3ea6fb57863",
		question: "<p>Как запустить программу под отладчиком в Visual Studio?</p>\n",
		answer: "<p>Клавишей F5. Кроме того можно подключиться к уже запущенному процессу через Debug/Attach Process</p>\n",
		unitTitle: "Ошибки",
		rate: RateTypes["rate2"],
		unitId: "6c13729e-817b-a437-b9d3-275c01f8f4a8",
		theorySlidesIds: [],
		lastRateIndex: 203,
		theorySlides: []
	}, {
		id: "28f3561e-9372-492a-875c-4d4b03b4a305",
		question: "<p>Для чего применяется слово <code>default</code> в выражении <code>switch</code>?</p>\n",
		answer: "<p>Чтобы указать инструкции, которые надо выполнить, если ни один из <code>case</code> не подошел.</p>\n",
		unitTitle: "Ветвления",
		rate: RateTypes["rate5"],
		unitId: "148775ee-9ffa-8932-64d6-d64380484169",
		theorySlidesIds: [],
		lastRateIndex: 111,
		theorySlides: []
	}, {
		id: "8469b0ef-c676-450e-aac7-89b34ef18785",
		question: "<p>Какие есть возможности сократить этот код:</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">if (a)\r\n    return 25;\r\nelse\r\n    return 0;</textarea>",
		answer: "<p>Убрать <code>else</code> или переписать с помощью тернарного оператора в одно выражение</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">return a ? 25 : 0;</textarea>",
		unitTitle: "Ветвления",
		rate: RateTypes["rate3"],
		unitId: "148775ee-9ffa-8932-64d6-d64380484169",
		theorySlidesIds: [],
		lastRateIndex: 60,
		theorySlides: []
	}, {
		id: "bc7a106e-51e4-4d13-9b05-c16359966e8a",
		question: "<p>Что может находиться внутри круглых скобок выражения <code>if</code></p>\n",
		answer: "<p>Любое выражение с типом <code>bool</code></p>\n",
		unitTitle: "Ветвления",
		rate: RateTypes["rate5"],
		unitId: "148775ee-9ffa-8932-64d6-d64380484169",
		theorySlidesIds: [],
		lastRateIndex: 116,
		theorySlides: []
	}, {
		id: "BAB44484-2045-446B-BF8B-0B8C8176D07E",
		question: "<p>Как выбрать между конструкциями <code>if</code>, <code>? :</code> и <code>switch</code>?</p>\n",
		answer: "<ol>\n<li>Если в зависимости от условия нужно выбрать одно из двух значений — использовать тернарный оператор <code>condition ? value1 : value2</code>.</li>\n<li>Если нужно выбрать один из многих вариантов в зависимости от некоторого значения — подойдёт <code>switch</code>.</li>\n<li>В остальных случаях нужно использовать <code>if</code>.</li>\n</ol>\n",
		unitTitle: "Ветвления",
		rate: RateTypes["rate4"],
		unitId: "148775ee-9ffa-8932-64d6-d64380484169",
		theorySlidesIds: [],
		lastRateIndex: 63,
		theorySlides: []
	}, {
		id: "8D747C8F-10C7-420E-98B0-64C7B9889F14",
		question: "<p>Чем отличаются операторы <code>||</code> и <code>|</code>, а также <code>&amp;&amp;</code> и <code>&amp;</code>?</p>\n",
		answer: "<ol>\n<li>«Удвоенные» операторы не вычисляют второй аргумент, если результат полностью определяется первым аргументом.</li>\n<li>«Одинарные» вычисляют оба своих аргумента. Также они работают не только с типом <code>bool</code>, но и с числовыми типами данных, выполняя побитовые логические операции.</li>\n</ol>\n",
		unitTitle: "Ветвления",
		rate: RateTypes["rate4"],
		unitId: "148775ee-9ffa-8932-64d6-d64380484169",
		theorySlidesIds: [],
		lastRateIndex: 67,
		theorySlides: []
	}, {
		id: "8faa79ff-b453-45b2-8913-b6584c4c21ec",
		question: "<p>Что делает <code>continue</code> в цикле?</p>\n",
		answer: "<p>Переходит к следующей итерации текущего цикла.</p>\n",
		unitTitle: "Циклы",
		rate: RateTypes["rate2"],
		unitId: "d083f956-8fa4-6024-04da-cfc8e17bd9db",
		theorySlidesIds: [],
		lastRateIndex: 124,
		theorySlides: []
	}, {
		id: "acf087b1-8c92-4cba-8a30-90e5b0c01be2",
		question: "<p>Как сделать бесконечный цикл на <code>for</code>?</p>\n",
		answer: "\n<textarea class=\"code code-sample\" data-lang=\"csharp\">for ( ; ; )\r\n{ ... }</textarea>",
		unitTitle: "Циклы",
		rate: RateTypes["rate2"],
		unitId: "d083f956-8fa4-6024-04da-cfc8e17bd9db",
		theorySlidesIds: [],
		lastRateIndex: 125,
		theorySlides: []
	}, {
		id: "6FB4525E-10B4-4559-8C66-DEC3A624280B",
		question: "<p>Когда стоит использовать цикл <code>for</code>, а не <code>while</code>?</p>\n",
		answer: "<ol>\n<li>Заранее известно сколько раз нужно повторить действие — используйте <strong>for</strong>.</li>\n<li>Есть переменная, изменяемая каждую итерацию и у нее есть понятный смысл — используйте <strong>for</strong>.</li>\n</ol>\n",
		unitTitle: "Циклы",
		rate: RateTypes["rate3"],
		unitId: "d083f956-8fa4-6024-04da-cfc8e17bd9db",
		theorySlidesIds: [],
		lastRateIndex: 126,
		theorySlides: []
	}, {
		id: "3A802D29-1FC6-40EC-A26A-7AB599069A67",
		question: "<p>Как можно досрочно прервать цикл <code>for</code>?</p>\n",
		answer: "<p>Оператор <code>break</code> прерывает выполнение любого цикла, в котором он вызывается.</p>\n",
		unitTitle: "Циклы",
		rate: RateTypes["rate1"],
		unitId: "d083f956-8fa4-6024-04da-cfc8e17bd9db",
		theorySlidesIds: [],
		lastRateIndex: 127,
		theorySlides: []
	}, {
		id: "0F8E776D-72E5-4CB3-9350-2EB0405D9A7E",
		question: "<p>Можно ли с помощью <code>while</code> сэмулировать цикл <code>for</code>? А наоборот?</p>\n",
		answer: "<p>Можно и в ту и в другую сторону, вот так:</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">// for → while:\r\nfor (int i=0; i<10; i++)\r\n  Do(i);\r\n// ↓\r\nint i=0;\r\nwhile (i<10) \r\n  Do(i++);\r\n          \r\n// while → for:\r\nwhile (Condition())\r\n  Do();\r\n// ↓\r\nfor(;Condition();)\r\n  Do(i);</textarea>",
		unitTitle: "Циклы",
		rate: RateTypes["rate5"],
		unitId: "d083f956-8fa4-6024-04da-cfc8e17bd9db",
		theorySlidesIds: [],
		lastRateIndex: 123,
		theorySlides: []
	}, {
		id: "1e0ab045-ea16-405a-9aa0-5e8e8ba77fef",
		question: "<p>Как получить последний элемент в массиве?</p>\n",
		answer: "<p>Использовать длину массива <code>xs[xs.Length - 1]</code></p>\n",
		unitTitle: "Массивы",
		rate: RateTypes["rate2"],
		unitId: "c777829b-7226-9049-ddf9-895234334f3f",
		theorySlidesIds: [],
		lastRateIndex: 162,
		theorySlides: []
	}, {
		id: "ff7e1847-c5e2-4871-8aed-da93edb0332f",
		question: "<p>Чему равно выражение</p>\n<textarea class='code code-sample' data-lang='csharp'>new int[] { 1, 2, 3 } == new int[] { 1, 2, 3 }</textarea>\n",
		answer: "<p><code>false</code>. Массивы сравниваются по ссылкам. Здесь массивы — это 2 разных объекта, поэтому ссылки не\r\n                будут равны.</p>\n",
		unitTitle: "Массивы",
		rate: RateTypes["rate2"],
		unitId: "c777829b-7226-9049-ddf9-895234334f3f",
		theorySlidesIds: [],
		lastRateIndex: 154,
		theorySlides: []
	}, {
		id: "35a9048a-41c2-45de-9be4-5eb86812a6bd",
		question: "<p>Какое значение получить быстрее в массиве из 1000 элементов: <code>values[0]</code> или <code>values[999]</code>?</p>\n",
		answer: "<p>Одинаково. Индекс всего лишь означает сдвиг адреса, из которого будет прочитано значение.</p>\n",
		unitTitle: "Массивы",
		rate: RateTypes["rate3"],
		unitId: "c777829b-7226-9049-ddf9-895234334f3f",
		theorySlidesIds: [],
		lastRateIndex: 155,
		theorySlides: []
	}, {
		id: "dad7ad24-7bee-4b52-a925-369c60e5a019",
		question: "<p>Где хранится строка? В куче или на стеке?</p>\n",
		answer: "<p>В куче. Как правило все значения, которые могут занимать значительный объем данных (строки,\r\n                массивы) хранятся в куче, а не на стеке.</p>\n",
		unitTitle: "Массивы",
		rate: RateTypes["rate4"],
		unitId: "c777829b-7226-9049-ddf9-895234334f3f",
		theorySlidesIds: [],
		lastRateIndex: 156,
		theorySlides: []
	}, {
		id: "FAF33DE8-1A32-4BBE-A0F0-8C4A44F5B51F",
		question: "<p>В чем отличие массива массивов от двумерного массива?</p>\n",
		answer: "<ol>\n<li>Каждый массив в массиве массивов нужно отдельно инициализировать.</li>\n<li>Каждый массив в массиве массивов может быть своей длины.</li>\n<li>Все элементы двумерного массива располагаются в памяти подряд.</li>\n</ol>\n",
		unitTitle: "Массивы",
		rate: RateTypes["rate4"],
		unitId: "c777829b-7226-9049-ddf9-895234334f3f",
		theorySlidesIds: [],
		lastRateIndex: 157,
		theorySlides: []
	}, {
		id: "53482D41-D6DC-4997-98C0-742CF3FFD8C6",
		question: "<p>Какие операции допустимы с массивами?</p>\n",
		answer: "<ol>\n<li>Узнать длину массива.</li>\n<li>Прочитать элемент массива по индексу.</li>\n<li>Записать элемент в ячейку с заданным индексом.</li>\n<li>Невозможно: изменить длину массива, добавить новый элемент в массив, удалить элемент из массива.</li>\n</ol>\n",
		unitTitle: "Массивы",
		rate: RateTypes["rate4"],
		unitId: "c777829b-7226-9049-ddf9-895234334f3f",
		theorySlidesIds: [],
		lastRateIndex: 158,
		theorySlides: []
	}, {
		id: "B63DA22A-A87F-4ED7-93F7-3C16479A87C0",
		question: "<p>Что делать, если необходимо добавить в массив ещё один элемент?</p>\n",
		answer: "<ol>\n<li>Придётся создать новый массив большего размера и скопировать в него все элементы исходного массива и новый элемент.</li>\n<li>Либо можно сразу создать массив большего размера, чем нужно, и в отдельной переменной хранить, сколько ячеек массива занято значениями. Но как только «запасные» ячейки закончатся, все равно придётся пересоздавать массив как в п.1.</li>\n</ol>\n",
		unitTitle: "Массивы",
		rate: RateTypes["rate4"],
		unitId: "c777829b-7226-9049-ddf9-895234334f3f",
		theorySlidesIds: [],
		lastRateIndex: 159,
		theorySlides: []
	}, {
		id: "c8da504b-817b-4c22-b2be-f05c9345cebc",
		question: "<p>В чем ошибка? Как исправить?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">File.ReadAllLines(\"C:\\doc.txt\")</textarea>",
		answer: "<p>Слеши считаются спец-символами экранирования. Либо их нужно экранировать (удвоить)\r\n<code>&quot;C:\\\\\\\\doc.txt&quot;</code>, либо использовать &quot;дословный&quot; формат строки\r\n<code>@&quot;C:\\doc.txt&quot;</code></p>\n",
		unitTitle: "Коллекции, строки, файлы",
		rate: RateTypes["rate5"],
		unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
		theorySlidesIds: [],
		lastRateIndex: 169,
		theorySlides: []
	}, {
		id: "36e47c38-af0d-41c7-8240-7da24e4c3f13",
		question: "<p>Чем отличается массив от списка (<code>List</code>)?</p>\n",
		answer: "<p>У массива фиксирована длина — она задаётся при создании массива и её нельзя поменять. Список увеличивает\r\nсвою длину по мере необходимости.\r\nВ отличие от массива у него есть метод <code>Add(item)</code></p>\n",
		unitTitle: "Коллекции, строки, файлы",
		rate: RateTypes["rate5"],
		unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
		theorySlidesIds: [],
		lastRateIndex: 170,
		theorySlides: []
	}, {
		id: "17ece429-52ee-4509-a26e-c7d3a4bcafe2",
		question: "<p>Для чего нужно экранирование символов в строке? Какие символы нужно экранировать?</p>\n",
		answer: "<p>Как минимум нужно экранировать кавычки, переводы строк и сами слеши. Потому что без экранирования\r\n                они обозначают что-то другое: кавычка заканчивает строку, перевод строк не может содержаться внутри\r\n                строки, а слеши используются для экранирования других символов.</p>\n",
		unitTitle: "Коллекции, строки, файлы",
		rate: RateTypes["rate3"],
		unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
		theorySlidesIds: [],
		lastRateIndex: 165,
		theorySlides: []
	}, {
		id: "5a8fadc8-bad4-482b-9f24-fef0df4aec72",
		question: "<p>В каких случаях нужно использовать <code>StringBuilder</code>?</p>\n",
		answer: "<p>Когда надо собрать длинную строчку из большого числа маленьких. Например, из массива строк.\r\n                Другие способы эффективно собрать строку из нескольких маленьких: <code>string.Concat</code>, <code>string.Join</code> и\r\n                <code>string.Format</code>.</p>\n",
		unitTitle: "Коллекции, строки, файлы",
		rate: RateTypes["rate4"],
		unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
		theorySlidesIds: [],
		lastRateIndex: 166,
		theorySlides: []
	}, {
		id: "e2a7effc-0b16-4194-bc2c-05ba77be8ae0",
		question: "<p>Что общего у <code>StringBuilder</code> и <code>List</code>?</p>\n",
		answer: "<p>Оба выделяют память с запасом и динамически увеличивают свой размер по мере добавления новых\r\nданных. Оба предоставляют методы для добавления, удаления и модификации содержимого.</p>\n",
		unitTitle: "Коллекции, строки, файлы",
		rate: RateTypes["rate5"],
		unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
		theorySlidesIds: [],
		lastRateIndex: 167,
		theorySlides: []
	}, {
		id: "15abf901-49d1-43b1-bcec-ba7dd1605d8d",
		question: "<p>В чем ошибка?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">string a;\r\nvar parts = a.Split(\" \");</textarea>",
		answer: "<p>Переменная <code>a</code> не инициализирована. \r\n                Будет ошибка компиляции, указывающая на попытку использовать неинициализированную переменную.</p>\n",
		unitTitle: "Коллекции, строки, файлы",
		rate: RateTypes["rate5"],
		unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
		theorySlidesIds: [],
		lastRateIndex: 168,
		theorySlides: []
	}, {
		id: "0f592c6d-3ddb-471b-9ac8-409589e77329",
		question: "<p>Что такое автоматическое тестирование?</p>\n",
		answer: "<p>Процесс, когда корректность одной программы проверяется другой программой\r\nпутём запуска тестируемой программы в разных ситуациях и сравнения ее результата с эталоном.</p>\n",
		unitTitle: "Тестирование",
		rate: RateTypes["rate1"],
		unitId: "0299fc2c-1fdc-d85d-d654-4a6a1353f64d",
		theorySlidesIds: [],
		lastRateIndex: 183,
		theorySlides: []
	}, {
		id: "345f7f9c-3f35-4bcb-b99a-bd9fc7478274",
		question: "<p>Как создать библиотеку в C#?</p>\n",
		answer: "<p>Создать отдельный проект и указать у него тип <em>Class Library</em></p>\n",
		unitTitle: "Тестирование",
		rate: RateTypes["rate1"],
		unitId: "0299fc2c-1fdc-d85d-d654-4a6a1353f64d",
		theorySlidesIds: [],
		lastRateIndex: 184,
		theorySlides: []
	}, {
		id: "33630124-0160-4559-9b15-cea0c504fc09",
		question: "<p>Как бороться с дублирование кода в тестах?</p>\n",
		answer: "<p>Выделить специальный метод, который в параметрах принимает данные и ожидаемый результат, а внутри делает проверку. В тестах использовать его.</p>\n",
		unitTitle: "Тестирование",
		rate: RateTypes["rate1"],
		unitId: "0299fc2c-1fdc-d85d-d654-4a6a1353f64d",
		theorySlidesIds: [],
		lastRateIndex: 185,
		theorySlides: []
	}, {
		id: "f0e7a5d2-2925-4eec-9f5d-51641042cdda",
		question: "<p>Что такое покрытие тестами?</p>\n",
		answer: "<p>Доля строк кода тестируемой программы, выполненных при запуске комплекта тестов.</p>\n",
		unitTitle: "Тестирование",
		rate: RateTypes["rate1"],
		unitId: "0299fc2c-1fdc-d85d-d654-4a6a1353f64d",
		theorySlidesIds: [],
		lastRateIndex: 186,
		theorySlides: []
	}, {
		id: "ef2cca96-526a-4a18-805b-43a99fed444d",
		question: "<p>Приведите особенности функционального тестирования.</p>\n",
		answer: "<p>Функциональные тесты проверяют, что тестируемая часть программы работает в типовых случаях её использования.\r\nВозможно, проверяется согласованная работа нескольких модулей. Как правило, тесты разрабатываются методом черного ящика,\r\nтолько по спецификации, без использования знаний о деталях реализации.</p>\n",
		unitTitle: "Тестирование",
		rate: RateTypes["rate1"],
		unitId: "0299fc2c-1fdc-d85d-d654-4a6a1353f64d",
		theorySlidesIds: [],
		lastRateIndex: 187,
		theorySlides: []
	}, {
		id: "aee8e53c-26c9-438f-8a1e-ae9a0b0cf9e1",
		question: "<p>В классе объявлены следующие поля, но нигде нет кода инициализации полей. \r\nКакие значения лежат в полях в этом случае?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">string a;\r\nint b;\r\nint[] xs;\r\nbool f;</textarea><p>А если бы это были не поля класса, а переменные внутри метода?</p>\n",
		answer: "<p>Поля классов автоматически инициализируются значениями по умолчанию.\r\n<code>string a = null</code>,\r\n<code>int b = 0</code>,\r\n<code>int[] xs = null</code>,\r\n<code>bool f = false</code>.\r\nПеременные, в отличие от полей, не инициализируются значениями по умолчанию.\r\nИх нельзя использовать до инициализации, и за этим следит компилятор.</p>\n",
		unitTitle: "Тестирование",
		rate: RateTypes["rate5"],
		unitId: "0299fc2c-1fdc-d85d-d654-4a6a1353f64d",
		theorySlidesIds: [],
		lastRateIndex: 182,
		theorySlides: []
	}, {
		id: "9acda6fa-5e8e-4019-b060-2e2a5c88b8f1",
		question: "<p>Пусть <code>xs</code> — массив целых чисел.\r\n                Чему равна сложность поиска максимального элемента <code>Max(xs)</code>?\r\n                При каких условиях сложность может быть константной?</p>\n",
		answer: "<p>Сложность линейная, потому что придется перебрать все элементы массива. \r\nНо если массив отсортирован, то найти максимум можно за <span class='tex'>O(1)</span> — просто взять последний или первый элемент \r\nв зависимости от порядка сортировки.</p>\n",
		unitTitle: "Сложность алгоритмов",
		rate: RateTypes["rate5"],
		unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
		theorySlidesIds: [],
		lastRateIndex: 117,
		theorySlides: []
	}, {
		id: "3494eed3-d195-4563-bff3-8da88d4d39d3",
		question: "<p>Дайте определение временной сложности алгоритма</p>\n",
		answer: "<p>Это функция, которая размеру входных данных ставит в соответствие точную верхнюю грань количества\r\n                элементарных операций, которое требуется алгоритму на входах заданного размера.</p>\n",
		unitTitle: "Сложность алгоритмов",
		rate: RateTypes["rate5"],
		unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
		theorySlidesIds: [],
		lastRateIndex: 113,
		theorySlides: []
	}, {
		id: "a04fac9a-2176-4ead-be2a-6620b1569d8a",
		question: "<p>Дайте определение ёмкостной сложности алгоритма</p>\n",
		answer: "<p>Это функция, которая размеру входных данных ставит в соответствие точную верхнюю грань количества\r\n                дополнительной памяти, которое требуется алгоритму на входах заданного размера.</p>\n",
		unitTitle: "Сложность алгоритмов",
		rate: RateTypes["rate5"],
		unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
		theorySlidesIds: [],
		lastRateIndex: 112,
		theorySlides: []
	}, {
		id: "cf1168f2-9487-4ee9-a91c-0cbc6ccbf1d8",
		question: "<p>Зачем нужна <span class='tex'>O</span>-нотация для описания временной сложности алгоритма?</p>\n",
		answer: "<p>&quot;Количество элементарных операций&quot; в определении сложности в реальности очень условное\r\n                понятие. Процессоры устроены сложно и даже одна и та же операция может иметь разную стоимость в\r\n                зависимости от ситуации. Поэтому точное значение сложности не имеет смысла, а проще работать с оценкой\r\n                сложности.</p>\n",
		unitTitle: "Сложность алгоритмов",
		rate: RateTypes["rate5"],
		unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
		theorySlidesIds: [],
		lastRateIndex: 45,
		theorySlides: []
	}, {
		id: "5e0b7ab5-c336-4b5c-b07d-2e943599c54e",
		question: "<p>Чем <span class='tex'>\\Theta(n)</span> отличается от <span class='tex'>O(n)</span>?</p>\n",
		answer: "<p><span class='tex'>\\Theta(n)</span> — это точная оценка асимптотики, а <span class='tex'>O</span> — лишь оценка сверху.</p>\n",
		unitTitle: "Сложность алгоритмов",
		rate: RateTypes["rate1"],
		unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
		theorySlidesIds: [],
		lastRateIndex: 106,
		theorySlides: []
	}, {
		id: "f1babb90-2f3f-49d0-a796-6e4c7d3a043e",
		question: "<p>Какова сложность добавления элемента в начало <em>List</em>?</p>\n",
		answer: "<p><span class='tex'>O(n)</span>. <em>List</em> реализован поверх массива. Для вставки нового элемента в начало, придется сдвинуть\r\n                вправо все элементы <em>List</em> на одну позицию.</p>\n",
		unitTitle: "Сложность алгоритмов",
		rate: RateTypes["rate5"],
		unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
		theorySlidesIds: [],
		lastRateIndex: 115,
		theorySlides: []
	}, {
		id: "F569E6D7-1EA0-4ADA-BB93-8E2B03F845E6",
		question: "<p>В чём идея подхода «Разделяй и властвуй» к проектированию алгоритмов.</p>\n",
		answer: "<ol>\n<li>Разбить всю задачу на несколько подзадач меньшего размера.</li>\n<li>Научиться решать задачи для задач минимального размера.</li>\n<li>Решить каждую подзадачу тем же способом.</li>\n<li>Объединить решения подзадач в решение целой задачи.</li>\n</ol>\n",
		unitTitle: "Рекурсивные алгоритмы",
		rate: RateTypes["rate5"],
		unitId: "40de4f88-54d6-3c23-faee-0f9de37ad824",
		theorySlidesIds: ["c233ec6f-5803-48e5-a67c-b5824baa7582"],
		lastRateIndex: 174,
		theorySlides: [{
			slug: "Razdelyay_i_vlastvuy_c233ec6f-5803-48e5-a67c-b5824baa7582",
			title: "Разделяй и властвуй"
		}]
	}, {
		id: "71FC0E76-C79F-4E1B-8895-DB541949A1D1",
		question: "<p>Что может означать исключение StackOverflowException в рекурсивной программе?</p>\n",
		answer: "<p>Скорее всего программа вошла в бесконечную рекурсию.\r\nЭто свидетельствует об ошибке в программе: программист не предусмотрел условие выхода из рекурсии или ошибся в этом условии.</p>\n<p>Это исключение может появляться и в случае, когда бесконечной рекурсии нет, но стек вызовов переполнился.\r\nДля этого нужно, чтобы было десятки тысяч вложенных вызовов методов, что бывает редко.\r\nПоэтому на практике это исключение скорее всего обозначает именно бесконечную рекурсию.</p>\n",
		unitTitle: "Рекурсивные алгоритмы",
		rate: RateTypes["rate5"],
		unitId: "40de4f88-54d6-3c23-faee-0f9de37ad824",
		theorySlidesIds: ["caec41b0-3166-40c0-9ded-3941c7f0b91d"],
		lastRateIndex: 175,
		theorySlides: [{ slug: "Rekursiya_caec41b0-3166-40c0-9ded-3941c7f0b91d", title: "Рекурсия" }]
	}, {
		id: "8f35c426-4106-470b-b416-6a2651f02dc6",
		question: "<p>Как можно посчитать сумму элементов массива без циклов и библиотечных функций?</p>\n",
		answer: "<p>С помощью рекурсии. Сумма пустого массива — 0.\r\nСумма непустого — это первый элемент + сумма оставшейся части массива.</p>\n",
		unitTitle: "Рекурсивные алгоритмы",
		rate: RateTypes["rate5"],
		unitId: "40de4f88-54d6-3c23-faee-0f9de37ad824",
		theorySlidesIds: [],
		lastRateIndex: 176,
		theorySlides: []
	}, {
		id: "f5547de7-4073-4939-bdae-016f1a7cbb9e",
		question: "<p>Сортировка пузырьком. Опишите идею алгоритма, дайте оценку временной и ёмкостной сложности.</p>\n",
		answer: "<p>Последовательно каждое значение в массиве передвигается к началу (всплывает) до нужной позиции.\r\nВременная сложность <span class='tex'>O(n^{2})</span>, ёмкостная — <span class='tex'>O(1)</span>.</p>\n",
		unitTitle: "Поиск и сортировка",
		rate: RateTypes["rate5"],
		unitId: "e4df2f6b-dc5d-3cc1-1467-0edcc93c211a",
		theorySlidesIds: [],
		lastRateIndex: 188,
		theorySlides: []
	}, {
		id: "047630b0-98cd-49a2-bc7c-885be3aa4af8",
		question: "<p>Сортировка слиянием. Опишите идею алгоритма, дайте оценку временной и ёмкостной сложности.</p>\n",
		answer: "<p>Массив разбивается на две примерно равные части, каждая сортируется этим же алгоритмом, затем\r\nотсортированные части объединяются. Временная сложность <span class='tex'>O(n \\log n)</span>. Емкостная — <span class='tex'>O(n)</span>.</p>\n",
		unitTitle: "Поиск и сортировка",
		rate: RateTypes["rate5"],
		unitId: "e4df2f6b-dc5d-3cc1-1467-0edcc93c211a",
		theorySlidesIds: [],
		lastRateIndex: 189,
		theorySlides: []
	}, {
		id: "f882933c-52d6-4fa2-943d-243b7cf5fbf2",
		question: "<p>Быстрая сортировка. Опишите идею алгоритма, дайте оценку временной и ёмкостной сложности.</p>\n",
		answer: "<p>Выбирается элемент - разделитель, после чего элементы массива переставляются так, чтобы слева\r\n                оказались элементы меньшие разделителя, а справа — большие. Дальше на каждой из двух частей вызываем\r\n                сортировку рекурсивно. Временная сложность <span class='tex'>O(n^{2})</span>, но в среднем случае <span class='tex'>O(n \\log n)</span>. Ёмкостная — <span class='tex'>O(n)</span> на\r\n                хранение данных на стеке при рекурсивных вызовах.</p>\n",
		unitTitle: "Поиск и сортировка",
		rate: RateTypes["rate5"],
		unitId: "e4df2f6b-dc5d-3cc1-1467-0edcc93c211a",
		theorySlidesIds: [],
		lastRateIndex: 190,
		theorySlides: []
	}, {
		id: "d4be5a0a-e4a0-4301-bd3d-a087a44c32e4",
		question: "<p>Можно ли найти элемент в отсортированном массиве быстрее, чем за линейный просмотр?</p>\n",
		answer: "<p>Да, с помощью бинарного поиска.</p>\n",
		unitTitle: "Поиск и сортировка",
		rate: RateTypes["rate5"],
		unitId: "e4df2f6b-dc5d-3cc1-1467-0edcc93c211a",
		theorySlidesIds: [],
		lastRateIndex: 191,
		theorySlides: []
	}, {
		id: "51237036-bd9f-4f60-bc09-2c2f609eae9a",
		question: "<p>Идея и временная сложность бинарного поиска</p>\n",
		answer: "<p>Сравнивать элемент из середины отсортированного массива с искомым и в зависимости от результата сравнения искать в\r\n                левой или правой половине массива.\r\n                Временная сложность — <span class='tex'>O(\\log n)</span>.</p>\n",
		unitTitle: "Поиск и сортировка",
		rate: RateTypes["rate5"],
		unitId: "e4df2f6b-dc5d-3cc1-1467-0edcc93c211a",
		theorySlidesIds: [],
		lastRateIndex: 192,
		theorySlides: []
	}, {
		id: "3e280ae7-3a38-4dd7-86b6-26cbcd40af28",
		question: "<p>Почему быстрая сортировка называется быстрой?</p>\n",
		answer: "<p>Несмотря на то, что ее сложность <span class='tex'>O(n^{2})</span>, в среднем она работает быстрее, чем сортировка слиянием и\r\n                многие другие сортировки.</p>\n",
		unitTitle: "Поиск и сортировка",
		rate: RateTypes["rate5"],
		unitId: "e4df2f6b-dc5d-3cc1-1467-0edcc93c211a",
		theorySlidesIds: [],
		lastRateIndex: 193,
		theorySlides: []
	}, {
		id: "93D78E4A-1961-4BA0-855F-EDD7C7413F52",
		question: "<p>Как определить класс на C#?</p>\n",
		answer: "\n<textarea class=\"code code-sample\" data-lang=\"csharp\">class MyClass { }</textarea>",
		unitTitle: "Основы ООП",
		rate: RateTypes["rate5"],
		unitId: "b8ff29db-7416-c2f6-aa37-7e58a96ed597",
		theorySlidesIds: [],
		lastRateIndex: 118,
		theorySlides: []
	}, {
		id: "65C8C5F7-BDF4-4EFB-A982-031DA66DB0CF",
		question: "<p>В чем разница между классом и объектом?</p>\n",
		answer: "<p>Объект — это экземпляр класса. Класс — это «чертеж» объекта.\r\nКласс — это тип. Объект — это значение переменной этого типа.</p>\n",
		unitTitle: "Основы ООП",
		rate: RateTypes["rate3"],
		unitId: "b8ff29db-7416-c2f6-aa37-7e58a96ed597",
		theorySlidesIds: [],
		lastRateIndex: 71,
		theorySlides: []
	}, {
		id: "1A6F47DD-1D9C-474C-B320-BF418088040C",
		question: "<p>Что такое поле класса? Какие они бывают?</p>\n",
		answer: "<p>Поле класса — это именованное значение некоторого типа. Аналог переменной, но привязанной к классу, в котором определено поле.</p>\n<p>Поля бывают статическими и нестатическими (динамическими). \r\nНестатические поля хранят своё значение для каждого объекта этого класса.\r\nСтатические поля хранят одно значение для всех объектов этого класса.</p>\n",
		unitTitle: "Основы ООП",
		rate: RateTypes["rate5"],
		unitId: "b8ff29db-7416-c2f6-aa37-7e58a96ed597",
		theorySlidesIds: [],
		lastRateIndex: 77,
		theorySlides: []
	}, {
		id: "A770FD77-E041-407E-A3C9-0246CAE9B18B",
		question: "<p>В чем ошибка?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">string a = null;\r\nvar parts = a.Split(\" \");</textarea><p>Как написать SafeSplit, чтобы не было NullReferenceException в коде</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">string a = null;\r\nvar parts = a.SafeSplit(\" \");</textarea>",
		answer: "<p>В переменной <code>a</code> лежит <code>null</code> и поэтому будет <code>NullReferenceException</code>. <code>SafeSplit</code> должен быть методом-расширения.\r\nВ этом случае он будет статическим и в нем можно обработать ситуацию <code>a == null</code>.\r\nПри этом его можно будет вызвать как указано.</p>\n",
		unitTitle: "Основы ООП",
		rate: RateTypes["rate4"],
		unitId: "b8ff29db-7416-c2f6-aa37-7e58a96ed597",
		theorySlidesIds: [],
		lastRateIndex: 66,
		theorySlides: []
	}, {
		id: "AC0EC219-3DCD-4ABE-A59D-1098C9ED4F28",
		question: "<p>Можно ли одном проекте создать два класса с одним именем?</p>\n",
		answer: "<p>Да, но для этого нужно разместить их в разных пространствах имен (namespaces)</p>\n",
		unitTitle: "Основы ООП",
		rate: RateTypes["rate5"],
		unitId: "b8ff29db-7416-c2f6-aa37-7e58a96ed597",
		theorySlidesIds: [],
		lastRateIndex: 22,
		theorySlides: []
	}, {
		id: "E74FF50B-5151-4C48-A1B3-1206E0AB8917",
		question: "<p>Что такое тип-интерфейс в языке C#?</p>\n",
		answer: "<p>Контракт, обязательство класса содержать некоторые методы и свойства.</p>\n",
		unitTitle: "Наследование",
		rate: RateTypes["rate4"],
		unitId: "8fe5a2fc-fe15-a2a6-87b8-74f4f36af51d",
		theorySlidesIds: [],
		lastRateIndex: 109,
		theorySlides: []
	}, {
		id: "2F491945-CE44-4C23-A39D-562A5747ACDB",
		question: "<p>Что такое наследование и зачем оно нужно?</p>\n",
		answer: "<p>Наследование позволяет определить дочерний класс, который использует (наследует), расширяет или изменяет возможности родительского класса. \r\nКласс, члены которого наследуются, называется базовым классом. \r\nКласс, который наследует члены базового класса, называется производным классом.\r\nНаследование воплощает идею «быть частным случаем чего-то».</p>\n",
		unitTitle: "Наследование",
		rate: RateTypes["rate2"],
		unitId: "8fe5a2fc-fe15-a2a6-87b8-74f4f36af51d",
		theorySlidesIds: [],
		lastRateIndex: 107,
		theorySlides: []
	}, {
		id: "FD016719-5E82-4718-B736-D532D6F1B180",
		question: "<p>Что такое upcast?</p>\n",
		answer: "<p>Приведение значения к родительскому типу. Объект не меняется — меняется точка зрения на него.</p>\n",
		unitTitle: "Наследование",
		rate: RateTypes["rate3"],
		unitId: "8fe5a2fc-fe15-a2a6-87b8-74f4f36af51d",
		theorySlidesIds: [],
		lastRateIndex: 108,
		theorySlides: []
	}, {
		id: "234A1115-F9BF-4692-B95B-30895C72C8F8",
		question: "<p>Что такое полиморфизм и зачем он нужен?</p>\n",
		answer: "<p>Полиморфизм позволяет объектам производного класса обрабатываться как объектам базового класса или интерфейса. \r\nТаким образом, алгоритмы использующие в работе некоторый интерфейс можно применять ко всем классам-наследниками.\r\nПри этом виртуальные методы и реализация интерфейса в каждом наследнике может быть различной.</p>\n",
		unitTitle: "Наследование",
		rate: RateTypes["rate5"],
		unitId: "8fe5a2fc-fe15-a2a6-87b8-74f4f36af51d",
		theorySlidesIds: [],
		lastRateIndex: 110,
		theorySlides: []
	}, {
		id: "08565E4F-6A6B-4EBD-9AB3-296A33E223E5",
		question: "<p>Что означает ключевое слово private?</p>\n",
		answer: "<p>Доступ к члену класса, помеченному модификатором private, возможен только из кода в этом же классе.</p>\n",
		unitTitle: "Целостность данных",
		rate: RateTypes["rate4"],
		unitId: "1557d92c-68e6-63d0-69ee-414354353685",
		theorySlidesIds: [],
		lastRateIndex: 150,
		theorySlides: []
	}, {
		id: "CC436168-DFD3-4528-89A8-DA77D57F47B7",
		question: "<p>Как модифицировать значения private-полей извне класса?</p>\n",
		answer: "<p>Напрямую — никак. Надо использовать методы класса, предусмотренные его автором, которые изменят поля, гарантируя целостность данных.</p>\n",
		unitTitle: "Целостность данных",
		rate: RateTypes["rate2"],
		unitId: "1557d92c-68e6-63d0-69ee-414354353685",
		theorySlidesIds: [],
		lastRateIndex: 145,
		theorySlides: []
	}, {
		id: "41697712-6937-4C09-A96E-DB54C210887A",
		question: "<p>Что такое отложенная ошибка?</p>\n",
		answer: "<p>Это нарушение целостности данных, приводящее к ошибке не в момент нарушения, а намного позже.</p>\n",
		unitTitle: "Целостность данных",
		rate: RateTypes["rate3"],
		unitId: "1557d92c-68e6-63d0-69ee-414354353685",
		theorySlidesIds: [],
		lastRateIndex: 146,
		theorySlides: []
	}, {
		id: "7F4BA6E3-1551-4BC9-A6DA-4AE054654CE5",
		question: "<p>Что такое свойство?</p>\n",
		answer: "<p>Это один или два метода: getter и setter, которые можно синтаксически использовать как поле, т.е. получать и присваивать ему значения.\r\nВ C# синтаксически обращение к свойству выглядит как обращение к полю, но фактически каждое обращение — это вызов getter или setter метода.</p>\n",
		unitTitle: "Целостность данных",
		rate: RateTypes["rate4"],
		unitId: "1557d92c-68e6-63d0-69ee-414354353685",
		theorySlidesIds: [],
		lastRateIndex: 147,
		theorySlides: []
	}, {
		id: "CE19B909-13DE-4D3B-82B3-350342E18E42",
		question: "<p>Сколько полей и методов будет сгенерировано компилятором при обработке этого свойства и какой у них будет уровень доступа (private, public)?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">public string Name {get; private set;}</textarea>",
		answer: "<p>Одно приватное поле, один публичный getter-метод, один приватный setter-метод.</p>\n",
		unitTitle: "Целостность данных",
		rate: RateTypes["rate5"],
		unitId: "1557d92c-68e6-63d0-69ee-414354353685",
		theorySlidesIds: [],
		lastRateIndex: 132,
		theorySlides: []
	}, {
		id: "ED0B75C4-CBD8-4A6B-9577-BFDBFFDEBDE5",
		question: "<p>Сколько полей и методов будет сгенерировано компилятором при обработке этого свойства и какой у них будет уровень доступа (private, public)?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">public string Surname { get; }</textarea>",
		answer: "<p>Это свойство только для чтения. Присвоить значение этому свойству можно в конструкторе.\r\nБудут сгенерированы одно приватное поле и один публичный getter-метод.</p>\n",
		unitTitle: "Целостность данных",
		rate: RateTypes["rate2"],
		unitId: "1557d92c-68e6-63d0-69ee-414354353685",
		theorySlidesIds: [],
		lastRateIndex: 151,
		theorySlides: []
	}, {
		id: "8BA45CC5-534B-4DAF-B0AA-D3C8FA7D750E",
		question: "<p>Как вызвать из одного конструктора другой конструктор того же класса, чтобы не дублировать код?</p>\n",
		answer: "<p>Используя this, например, так : Rectangle(int width, int height) : this(0, 0, width, height)</p>\n",
		unitTitle: "Целостность данных",
		rate: RateTypes["rate3"],
		unitId: "1557d92c-68e6-63d0-69ee-414354353685",
		theorySlidesIds: [],
		lastRateIndex: 152,
		theorySlides: []
	}, {
		id: "66F6FB62-32E3-46B5-8D9B-883F4A6DE71D",
		question: "<p>Что означает модификатор readonly у поля?</p>\n",
		answer: "<p>Его значение нельзя модифицировать после того, как отработал конструктор.</p>\n",
		unitTitle: "Целостность данных",
		rate: RateTypes["rate1"],
		unitId: "1557d92c-68e6-63d0-69ee-414354353685",
		theorySlidesIds: [],
		lastRateIndex: 135,
		theorySlides: []
	}, {
		id: "2BB1D8AF-014E-4BE0-AC2D-0D5202A4D81E",
		question: "<p>В чем разница между константой и static readonly полем?</p>\n",
		answer: "<p>Разное время вычисления значения: \r\nconst вычисляется в момент компиляции, static поле в процессе работы программы (в момент, когда первый раз понадобился класс, содержащий константу).\r\nЗначение константы должно быть известно на момент компиляции, поэтому множество допустимых типов сильно ограничено.</p>\n",
		unitTitle: "Целостность данных",
		rate: RateTypes["rate1"],
		unitId: "1557d92c-68e6-63d0-69ee-414354353685",
		theorySlidesIds: [],
		lastRateIndex: 136,
		theorySlides: []
	}, {
		id: "9429A451-EE18-466F-9337-ACBF2BED8EFB",
		question: "<p>Когда выполняются статические конструкторы?</p>\n",
		answer: "<p>До того, как будут использованы статические или динамические поля или методы класса, но не обязательно в самом начале программы.</p>\n",
		unitTitle: "Целостность данных",
		rate: RateTypes["rate1"],
		unitId: "1557d92c-68e6-63d0-69ee-414354353685",
		theorySlidesIds: [],
		lastRateIndex: 137,
		theorySlides: []
	}, {
		id: "3EE93A8A-9AB4-4013-83B9-C546747535D2",
		question: "<p>В чем ошибка? Как исправить?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">struct A { public B b; }\r\nclass B { public int c; }\r\nvar a = new A();\r\nConsole.WriteLine(a.b.c)</textarea>",
		answer: "<p>В поле b находится null и поэтому будет NullReferenceException.</p>\n",
		unitTitle: "Структуры",
		rate: RateTypes["rate4"],
		unitId: "97940709-9f6d-03f4-2090-63bd08befcf1",
		theorySlidesIds: [],
		lastRateIndex: 206,
		theorySlides: []
	}, {
		id: "B2190C3E-4ADF-4EFE-B111-2DAC0E46908E",
		question: "<p>В чем ошибка в коде?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">public struct Point\r\n{\r\n    public Point()\r\n    {\r\n        X = 0;\r\n        Y = 0;\r\n    }\r\n    public int X;\r\n    public int Y;\r\n}</textarea>",
		answer: "<p>У структуры нельзя переопределить конструктор по умолчанию.</p>\n",
		unitTitle: "Структуры",
		rate: RateTypes["rate3"],
		unitId: "97940709-9f6d-03f4-2090-63bd08befcf1",
		theorySlidesIds: [],
		lastRateIndex: 205,
		theorySlides: []
	}, {
		id: "B98EC877-6E9E-4D4B-84B5-EC6E7BEFECE1",
		question: "<p>Что делать, если надо передать структуру в метод по ссылке?</p>\n",
		answer: "<p>Использовать <code>ref</code>.</p>\n",
		unitTitle: "Структуры",
		rate: RateTypes["rate5"],
		unitId: "97940709-9f6d-03f4-2090-63bd08befcf1",
		theorySlidesIds: [],
		lastRateIndex: 207,
		theorySlides: []
	}, {
		id: "A08AD9D2-927D-45AC-8CC8-937967449FCC",
		question: "<p>Что такое boxing?</p>\n",
		answer: "<p>Неявное копирование структуры в кучу. Например, при upcast в object или реализуемый интерфейс.</p>\n",
		unitTitle: "Структуры",
		rate: RateTypes["rate2"],
		unitId: "97940709-9f6d-03f4-2090-63bd08befcf1",
		theorySlidesIds: [],
		lastRateIndex: 208,
		theorySlides: []
	}, {
		id: "D8179FD0-F230-4677-BF8E-7422612CF233",
		question: "<p>Пусть Point — это структура, содержащая публичные поля X, Y и метод GetLength. В каких строчках есть ошибки и почему?</p>\n\n<textarea class=\"code code-sample\" data-lang=\"csharp\">Point p;\r\np.X = 5;\r\nvar length = p.GetLength();</textarea>",
		answer: "<p>Только в третьей. Потому что нельзя использовать методы не инициализированной полностью структуры. А вот присвоить значение полю можно.</p>\n",
		unitTitle: "Структуры",
		rate: RateTypes["rate5"],
		unitId: "97940709-9f6d-03f4-2090-63bd08befcf1",
		theorySlidesIds: [],
		lastRateIndex: 171,
		theorySlides: []
	}];

const infoByUnits: InfoByUnit[] = [
	{
		unitId: "e1beb629-6f24-279a-3040-cf111f91e764",
		unitTitle: "Первое знакомство с C#",
		unlocked: false,
		flashcardsIds: ["914883a9-d782-46af-8ffc-165a5a421866", "c57945a1-2877-4d92-b796-d247b9d0afbb", "e1e61f5b-d931-453b-8ee5-b488f8ad13e4", "7fd3939b-1bf6-424c-b211-685da14c1d46", "5e8be18d-1cfb-43c6-b7c5-6a4eddc4a369", "0c22338a-13d8-4854-bfc4-9e7afd52fa95", "97f894c7-4217-48d9-974d-bf88a6ab1d6d"],
		unratedFlashcardsCount: 0,
		cardsCount: 7,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_7e536978-d860-4cca-a690-da1032a30bd5"
	},
	{
		unitId: "6c13729e-817b-a437-b9d3-275c01f8f4a8",
		unitTitle: "Ошибки",
		unlocked: true,
		flashcardsIds: ["61de4e20-6327-4177-b13c-4dd6011b11ba", "aadabf1c-2736-4d1a-ab90-c3ea6fb57863"],
		unratedFlashcardsCount: 0,
		cardsCount: 2,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_a99c8ea4-1033-4394-bb87-98e146f56668"
	},
	{
		unitId: "148775ee-9ffa-8932-64d6-d64380484169",
		unitTitle: "Ветвления",
		unlocked: false,
		flashcardsIds: ["28f3561e-9372-492a-875c-4d4b03b4a305", "8469b0ef-c676-450e-aac7-89b34ef18785", "bc7a106e-51e4-4d13-9b05-c16359966e8a", "BAB44484-2045-446B-BF8B-0B8C8176D07E", "8D747C8F-10C7-420E-98B0-64C7B9889F14"],
		unratedFlashcardsCount: 0,
		cardsCount: 5,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_90d771b0-f8b0-443a-945f-2eeeec96a6c4"
	},
	{
		unitId: "d083f956-8fa4-6024-04da-cfc8e17bd9db",
		unitTitle: "Циклы",
		unlocked: true,
		flashcardsIds: ["8faa79ff-b453-45b2-8913-b6584c4c21ec", "acf087b1-8c92-4cba-8a30-90e5b0c01be2", "6FB4525E-10B4-4559-8C66-DEC3A624280B", "3A802D29-1FC6-40EC-A26A-7AB599069A67", "0F8E776D-72E5-4CB3-9350-2EB0405D9A7E"],
		unratedFlashcardsCount: 0,
		cardsCount: 5,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_7833adde-01f1-4912-8bbe-1c4856d222d7"
	},
	{
		unitId: "c777829b-7226-9049-ddf9-895234334f3f",
		unitTitle: "Массивы",
		unlocked: true,
		flashcardsIds: ["1e0ab045-ea16-405a-9aa0-5e8e8ba77fef", "ff7e1847-c5e2-4871-8aed-da93edb0332f", "35a9048a-41c2-45de-9be4-5eb86812a6bd", "dad7ad24-7bee-4b52-a925-369c60e5a019", "FAF33DE8-1A32-4BBE-A0F0-8C4A44F5B51F", "53482D41-D6DC-4997-98C0-742CF3FFD8C6", "B63DA22A-A87F-4ED7-93F7-3C16479A87C0"],
		unratedFlashcardsCount: 0,
		cardsCount: 7,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_a577bb95-e437-4f02-860d-6cbcb57b9ab2"
	},
	{
		unitId: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
		unitTitle: "Коллекции, строки, файлы",
		unlocked: true,
		flashcardsIds: ["c8da504b-817b-4c22-b2be-f05c9345cebc", "36e47c38-af0d-41c7-8240-7da24e4c3f13", "17ece429-52ee-4509-a26e-c7d3a4bcafe2", "5a8fadc8-bad4-482b-9f24-fef0df4aec72", "e2a7effc-0b16-4194-bc2c-05ba77be8ae0", "15abf901-49d1-43b1-bcec-ba7dd1605d8d"],
		unratedFlashcardsCount: 0,
		cardsCount: 6,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_d2ccb0f7-5f1a-42fd-9b04-90248a5ca9b4"
	},
	{
		unitId: "0299fc2c-1fdc-d85d-d654-4a6a1353f64d",
		unitTitle: "Тестирование",
		unlocked: true,
		flashcardsIds: ["0f592c6d-3ddb-471b-9ac8-409589e77329", "345f7f9c-3f35-4bcb-b99a-bd9fc7478274", "33630124-0160-4559-9b15-cea0c504fc09", "f0e7a5d2-2925-4eec-9f5d-51641042cdda", "ef2cca96-526a-4a18-805b-43a99fed444d", "aee8e53c-26c9-438f-8a1e-ae9a0b0cf9e1"],
		unratedFlashcardsCount: 0,
		cardsCount: 6,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_12bee77d-efaf-47b6-9e52-03038831f79c"
	},
	{
		unitId: "fab42d9c-db92-daec-e883-7d5424eb6c13",
		unitTitle: "Сложность алгоритмов",
		unlocked: true,
		flashcardsIds: ["9acda6fa-5e8e-4019-b060-2e2a5c88b8f1", "3494eed3-d195-4563-bff3-8da88d4d39d3", "a04fac9a-2176-4ead-be2a-6620b1569d8a", "cf1168f2-9487-4ee9-a91c-0cbc6ccbf1d8", "5e0b7ab5-c336-4b5c-b07d-2e943599c54e", "f1babb90-2f3f-49d0-a796-6e4c7d3a043e"],
		unratedFlashcardsCount: 0,
		cardsCount: 6,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_3d69c830-046b-468c-8556-14ed845bb50f"
	},
	{
		unitId: "40de4f88-54d6-3c23-faee-0f9de37ad824",
		unitTitle: "Рекурсивные алгоритмы",
		unlocked: true,
		flashcardsIds: ["F569E6D7-1EA0-4ADA-BB93-8E2B03F845E6", "71FC0E76-C79F-4E1B-8895-DB541949A1D1", "8f35c426-4106-470b-b416-6a2651f02dc6"],
		unratedFlashcardsCount: 0,
		cardsCount: 3,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_b24a1dd7-524d-466c-9603-57381ebc33ff"
	},
	{
		unitId: "e4df2f6b-dc5d-3cc1-1467-0edcc93c211a",
		unitTitle: "Поиск и сортировка",
		unlocked: true,
		flashcardsIds: ["f5547de7-4073-4939-bdae-016f1a7cbb9e", "047630b0-98cd-49a2-bc7c-885be3aa4af8", "f882933c-52d6-4fa2-943d-243b7cf5fbf2", "d4be5a0a-e4a0-4301-bd3d-a087a44c32e4", "51237036-bd9f-4f60-bc09-2c2f609eae9a", "3e280ae7-3a38-4dd7-86b6-26cbcd40af28"],
		unratedFlashcardsCount: 0,
		cardsCount: 6,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_8ad843d8-0bf9-4a4c-97fe-a6e02a2f842b"
	},
	{
		unitId: "b8ff29db-7416-c2f6-aa37-7e58a96ed597",
		unitTitle: "Основы ООП",
		unlocked: true,
		flashcardsIds: ["93D78E4A-1961-4BA0-855F-EDD7C7413F52", "65C8C5F7-BDF4-4EFB-A982-031DA66DB0CF", "1A6F47DD-1D9C-474C-B320-BF418088040C", "A770FD77-E041-407E-A3C9-0246CAE9B18B", "AC0EC219-3DCD-4ABE-A59D-1098C9ED4F28"],
		unratedFlashcardsCount: 0,
		cardsCount: 5,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_17d78630-c5fa-4c0d-aaad-1cf4edd7e1b0"
	},
	{
		unitId: "8fe5a2fc-fe15-a2a6-87b8-74f4f36af51d",
		unitTitle: "Наследование",
		unlocked: true,
		flashcardsIds: ["E74FF50B-5151-4C48-A1B3-1206E0AB8917", "2F491945-CE44-4C23-A39D-562A5747ACDB", "FD016719-5E82-4718-B736-D532D6F1B180", "234A1115-F9BF-4692-B95B-30895C72C8F8"],
		unratedFlashcardsCount: 0,
		cardsCount: 4,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_4bbee343-194e-4bb8-bf9a-f072f401e81f"
	},
	{
		unitId: "1557d92c-68e6-63d0-69ee-414354353685",
		unitTitle: "Целостность данных",
		unlocked: false,
		flashcardsIds: ["08565E4F-6A6B-4EBD-9AB3-296A33E223E5", "CC436168-DFD3-4528-89A8-DA77D57F47B7", "41697712-6937-4C09-A96E-DB54C210887A", "7F4BA6E3-1551-4BC9-A6DA-4AE054654CE5", "CE19B909-13DE-4D3B-82B3-350342E18E42", "ED0B75C4-CBD8-4A6B-9577-BFDBFFDEBDE5", "8BA45CC5-534B-4DAF-B0AA-D3C8FA7D750E", "66F6FB62-32E3-46B5-8D9B-883F4A6DE71D", "2BB1D8AF-014E-4BE0-AC2D-0D5202A4D81E", "9429A451-EE18-466F-9337-ACBF2BED8EFB"],
		unratedFlashcardsCount: 0,
		cardsCount: 10,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_149390cf-a8c6-4c47-b374-9004561704e5"
	},
	{
		unitId: "97940709-9f6d-03f4-2090-63bd08befcf1",
		unitTitle: "Структуры",
		unlocked: true,
		flashcardsIds: ["3EE93A8A-9AB4-4013-83B9-C546747535D2", "B2190C3E-4ADF-4EFE-B111-2DAC0E46908E", "B98EC877-6E9E-4D4B-84B5-EC6E7BEFECE1", "A08AD9D2-927D-45AC-8CC8-937967449FCC", "D8179FD0-F230-4677-BF8E-7422612CF233"],
		unratedFlashcardsCount: 0,
		cardsCount: 5,
		flashcardsSlideSlug: "Voprosy_dlya_samoproverki_47be3bfd-6df5-49ff-91d5-abfb78d85cbf"
	}];

const buildQuestionWithAnswer = (question: string, answer: string,): QuestionWithAnswer => {
	return {
		question: question,
		answer: answer
	};
};

const longQuestion = 'Если я спрошу очень большой вопрос, то сломается ли верстка? Если да, то почему? Как это поправить? Или не стоит? как думаешь? Почему?';

const longAnswer = 'Большой ответ также может сломать все, что захочет, должно быть стоит лучше приглядываться к стилям. И так же надо написать ещё 5 слов чтобы было побольше слов.';

const questionsWithAnswers: QuestionWithAnswer[] = [
	buildQuestionWithAnswer('Почему 1 больше 2?', 'Потому что 2 меньше 1... наверное'),
	buildQuestionWithAnswer('Это так?', 'Нет, не так'),
	buildQuestionWithAnswer(longQuestion, longAnswer),
];

const longText = `Идейные соображения высшего порядка, а также рамки и место обучения кадров обеспечивает широкому кругу (специалистов) участие в формировании соответствующий условий активизации. Разнообразный и богатый опыт начало повседневной работы по формированию позиции требуют определения и уточнения позиций, занимаемых участниками в отношении поставленных задач.
Повседневная практика показывает, что новая модель организационной деятельности требуют от нас анализа систем массового участия. Товарищи! консультация с широким активом представляет собой интересный эксперимент проверки позиций, занимаемых участниками в отношении поставленных задач. Таким образом консультация с широким активом позволяет оценить значение системы обучения кадров, соответствует насущным потребностям. Идейные соображения высшего порядка, а также постоянный количественный рост и сфера нашей активности способствует подготовки и реализации систем массового участия. Разнообразный и богатый опыт постоянное информационно-пропагандистское обеспечение нашей деятельности способствует подготовки и реализации направлений прогрессивного развития. Идейные соображения высшего порядка, а также реализация намеченных плановых заданий в значительной степени обуславливает создание направлений прогрессивного развития.`;


const guides: string[] = [...defaultGuides, longText, "short"];

export { flashcards, infoByUnits, questionsWithAnswers, guides, };
