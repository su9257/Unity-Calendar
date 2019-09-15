
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;
using System.Globalization;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class FlatCalendar : MonoBehaviour {

	/**
	 * Max day slots (DO NOT CHANGE THIS VALUE)
	 */
	public static readonly int max_day_slots = 37;



	/**
	 * List of Sprites
	 */
	public Sprite[] sprites;

	/**
	 * Current UI Style
	 */
	public int current_UiStyle;

	/**
	 * Event Structure Object
	 */ 
	public struct EventObj
	{
		public string name;
		public string description;

		public EventObj(string _name, string _description)
		{
			name 		= _name;
			description = _description;
		}

		public void print()
		{
			Debug.Log("Name Event: " + name + " Description Event: " + description);
		}
	}

	/**
	 * Time Structure Object 
	 */
	public struct TimeObj
	{
		public int    year;
		public int    month;
		public int    day;
		public int    totalDays;
		public string dayOfWeek;
        /// <summary>
        /// 这个日子对应的item偏移量
        /// </summary>
		public int    dayOffset;

		public TimeObj(int _year,int _month,int _day, int _totalDays, string _dayOfWeek, int _dayOffset)
		{
			year      = _year;
			month     = _month;
			day       = _day;
			totalDays = _totalDays;
			dayOffset = _dayOffset;
			dayOfWeek = _dayOfWeek;
		}

		public void print()
		{
			Debug.Log("Year:"+year+" Month:"+month+" Day:"+day+" Day of Week:"+dayOfWeek);

		}

	}


    /// <summary>
    /// 下一个月
    /// </summary>
	GameObject btn_nextMonth;
    /// <summary>
    /// 上一个月
    /// </summary>
	GameObject btn_prevMonth;
    /// <summary>
    /// 回到今天
    /// </summary>
	GameObject btn_calendar;
    /// <summary>
    /// 年的标签
    /// </summary>
	GameObject label_year;
    /// <summary>
    /// 月份的标签
    /// </summary>
	GameObject label_month;
    /// <summary>
    /// 显示星期几
    /// </summary>
	GameObject label_dayOfWeek;
    /// <summary>
    /// 显示哪一天
    /// </summary>
	GameObject label_dayNumber;
    /// <summary>
    /// 显示这一天还有事件的数量
    /// </summary>
	GameObject label_numberEvents;


	/**
	 * Current Time 
	 */
	public TimeObj currentTime;


	/*+
	 * Event List 
	 */
	public static Dictionary<int,Dictionary<int,Dictionary<int,List<EventObj>>>> events_list; // <Year,<Month,<Day,Number of Events>>>

	/**
	 * Delegate Callbacks 
	 */
	public delegate void Delegate_OnDaySelected(TimeObj time);
	public delegate void Delegate_OnEventSelected(TimeObj time, List<EventObj> evs);
	public delegate void Delegate_OnMonthChanged(TimeObj time);
	public delegate void Delegate_OnNowDay(TimeObj time);
	public Delegate_OnDaySelected   delegate_ondayselected;
	public Delegate_OnEventSelected delegate_oneventselected;
	public Delegate_OnMonthChanged  delegate_onmonthchanged;
	public Delegate_OnNowDay		 delegate_onnowday;


	// Use this for initialization
	public void initFlatCalendar()
	{
		// Getting ui references
		btn_nextMonth      = GameObject.Find("Right_btn");
		btn_prevMonth      = GameObject.Find("Left_btn");
		btn_calendar       = GameObject.Find("Calendar_Btn");
		label_year         = GameObject.Find("Year");
		label_month        = GameObject.Find("Month");
		label_dayOfWeek    = GameObject.Find("Day_Title1");
		label_dayNumber    = GameObject.Find("Day_Title2");
		label_numberEvents = GameObject.Find("NumberEvents");

        // Add Event Listeners添加事件监听器
        addEventsListener();

        // Apply UI Color style应用UI颜色样式
        FlatCalendarStyle.changeUIStyle(current_UiStyle);

        // Set current time设置当前时间
        setCurrentTime();

        // Initialize event list初始化事件列表
        events_list = new Dictionary<int, Dictionary<int, Dictionary<int,List<EventObj>>>>();

        // Update Calendar with Current Data用当前数据更新日历
        updateCalendar(currentTime.month,currentTime.year);

        // Mark Current Day马克目前天
        markSelectionDay(currentTime.day);

        // Update Label Event 更新标签事件
        updateUiLabelEvents(currentTime.year,currentTime.month,currentTime.day);
	}

	void Start ()
    {
        DateTime dateTime = DateTime.Now;
      string tempStr =   SolarToChineseLunisolarDate(dateTime);
        Debug.Log(tempStr);
        Debug.Log("123456"[5]);
    }

    /// <summary>
    /// 公历转为农历的函数
    /// </summary>
    /// <remarks>作者：DeltaCat</remarks>
    /// <example>网址：http://www.zu14.cn</example>
    /// <param name="solarDateTime">公历日期</param>
    /// <returns>农历的日期</returns>
    static string SolarToChineseLunisolarDate(DateTime solarDateTime)
    {
        System.Globalization.ChineseLunisolarCalendar cal = new System.Globalization.ChineseLunisolarCalendar();

        int year = cal.GetYear(solarDateTime);
        int month = cal.GetMonth(solarDateTime);
        int day = cal.GetDayOfMonth(solarDateTime);
        int leapMonth = cal.GetLeapMonth(year);
        string stringFormat = $"农历" +
            $"{"甲乙丙丁戊己庚辛壬癸"[(year - 4) % 10]}" +
            $"{"子丑寅卯辰巳午未申酉戌亥"[(year - 4) % 12]}" +
            $"（{"鼠牛虎兔龙蛇马羊猴鸡狗猪"[(year - 4) % 12]}）年" +
            $"{(month == leapMonth ? "闰" : "")}{"无正二三四五六七八九十冬腊"[leapMonth > 0 && leapMonth <= month ? month - 1 : month]}月" +
            $"{"初十廿三"[day / 10]}{"日一二三四五六七八九"[day % 10]}";
        //return string.Format("农历{0}{1}（{2}）年{3}{4}月{5}{6}"
        //    , "甲乙丙丁戊己庚辛壬癸"[(year - 4) % 10]
        //                    , "子丑寅卯辰巳午未申酉戌亥"[(year - 4) % 12]
        //                    , "鼠牛虎兔龙蛇马羊猴鸡狗猪"[(year - 4) % 12]
        //                    , month == leapMonth ? "闰" : ""
        //                    , "无正二三四五六七八九十冬腊"[leapMonth > 0 && leapMonth <= month ? month - 1 : month]
        //                    , "初十廿三"[day / 10]
        //                    , "日一二三四五六七八九"[day % 10]
        //                    );
        return stringFormat;
    }

    void Update () { }


	// 显示某年某月的日历并显示对应的标签
	public void updateCalendar(int month_number, int year)
	{
        // Populate day slots填充天槽
        populateAllSlot(month_number,year);

        // Update Year and Month Label更新年和月标签
        label_year.GetComponent<Text>().text      = "" + currentTime.year;
		label_month.GetComponent<Text>().text     = getMonthStringFromNumber(currentTime.month);
	}

    /// <summary>
    /// 重置当天的日历
    /// </summary>
	public void refreshCalendar()
	{
		populateAllSlot(currentTime.month,currentTime.year);
	}

    /* Get Month String from Montth Number 从Montth Number获取月份字符串
	 * 
	 * Example: Genuary <====> 1
	 */

    /// <summary>
    /// 从Montth Number获取月份字符串
    /// </summary>
    /// <param name="month_number"></param>
    /// <returns></returns>
    string getMonthStringFromNumber(int month_number)
	{
		string month = "";

		if(month_number == 1) month = "Genuary";
		if(month_number == 2) month = "February";
		if(month_number == 3) month = "March";
		if(month_number == 4) month = "April";
		if(month_number == 5) month = "May";
		if(month_number == 6) month = "June";
		if(month_number == 7) month = "July";
		if(month_number == 8) month = "August";
		if(month_number == 9) month = "September";
		if(month_number == 10) month = "October";
		if(month_number == 11) month = "November";
		if(month_number == 12) month = "December";

		return month;
	}

    /* 
	 * Get Day of Week From Year, Month and Day 从年、月、日开始计算一周的天数
	 * 
	 * Example: Monday <===> 2016,1,2
	 */
     /// <summary>
     /// 获取某一天是哪个星期
     /// </summary>
     /// <param name="year"></param>
     /// <param name="month"></param>
     /// <param name="day"></param>
     /// <returns></returns>
    string getDayOfWeek(int year, int month, int day)
	{
		System.DateTime dateValue = new System.DateTime(year,month,day);

		return dateValue.DayOfWeek.ToString();
	}

    /* 
	 * Get index of first slot where start day numeration 获取第一个插槽的索引，其中开始计算日期
	 */
     /// <summary>
     /// 获取某一个月的第一天从哪个索引开始
     /// </summary>
     /// <param name="year"></param>
     /// <param name="month"></param>
     /// <returns></returns>
    int getIndexOfFirstSlotInMonth(int year, int month)
	{
		int indexOfFirstSlot = 0;

		System.DateTime dateValue = new System.DateTime(year,month,1);
		string dayOfWeek          = dateValue.DayOfWeek.ToString();

		if(dayOfWeek == "Monday")    indexOfFirstSlot = 0;
		if(dayOfWeek == "Tuesday")   indexOfFirstSlot = 1;
		if(dayOfWeek == "Wednesday") indexOfFirstSlot = 2;
		if(dayOfWeek == "Thursday")  indexOfFirstSlot = 3;
		if(dayOfWeek == "Friday")    indexOfFirstSlot = 4;
		if(dayOfWeek == "Saturday")  indexOfFirstSlot = 5;
		if(dayOfWeek == "Sunday")    indexOfFirstSlot = 6;

		return indexOfFirstSlot;
	}

	/*
	 * 关闭所有item
	 */
     /// <summary>
     /// 关闭所有的Item
     /// </summary>
	void disableAllSlot()
	{
		for(int i = 0; i < max_day_slots; i++)
			disableSlot(i+1);
	}

	/* 
	 * Disable day slot
	 */
     /// <summary>
     /// 关闭对应的item
     /// </summary>
     /// <param name="numSlot"></param>
	void disableSlot(int numSlot)
	{
		GameObject day_slot = GameObject.Find("Slot_"+ (numSlot));
		day_slot.GetComponent<Button>().enabled = false;
		day_slot.GetComponent<Image>().enabled  = false;
		day_slot.GetComponent<Button>().GetComponentInChildren<Text>().enabled = false;
	}

    /// <summary>
    /// 开启 ，非选中状态
    /// </summary>
    /// <param name="numSlot"></param>
	void setNormalSlot(int numSlot)
	{
		GameObject day_slot = GameObject.Find("Slot_"+ (numSlot));
		day_slot.GetComponent<Button>().enabled = true;
		day_slot.GetComponent<Image>().enabled  = false;
		day_slot.GetComponent<Button>().GetComponentInChildren<Text>().enabled = true;
		day_slot.GetComponent<Button>().GetComponentInChildren<Text>().color = FlatCalendarStyle.color_dayTextNormal;
	}

    /// <summary>
    /// 设置有事件的日期着色
    /// </summary>
    /// <param name="numSlot"></param>
	void setEventSlot(int numSlot)
	{
		Sprite sprite       = Resources.Load<Sprite>("img/circle_filled");
		GameObject day_slot = GameObject.Find("Slot_"+ (numSlot));
		day_slot.GetComponent<Button>().enabled = true;
		day_slot.GetComponent<Image>().enabled  = true;
		day_slot.GetComponent<Image>().sprite   = sprite;
		day_slot.GetComponent<Image>().color    = FlatCalendarStyle.color_bubbleEvent;
		day_slot.GetComponent<Button>().GetComponentInChildren<Text>().enabled = true;
		day_slot.GetComponent<Button>().GetComponentInChildren<Text>().color = FlatCalendarStyle.color_dayTextEvent;
	}
    /// <summary>
    /// 填充所有Item 显示某年某月的日历
    /// </summary>
    /// <param name="monthNumber"></param>
    /// <param name="year"></param>
	void populateAllSlot(int monthNumber, int year)
	{
		// Disable all slots
		disableAllSlot();

		// Update slots
		for (int i = 0; i < currentTime.totalDays; i++)
		{	
			// Put text
			changeTextSlot(i+currentTime.dayOffset+1,""+(i+1));

			// Check if slot event
			if(checkEventExist(currentTime.year,currentTime.month,(i+1)))
				setEventSlot(i+currentTime.dayOffset+1);
			else
				setNormalSlot(i+currentTime.dayOffset+1);
		}
	}
    /// <summary>
    /// 更改对应Item上面的日期
    /// </summary>
    /// <param name="numSlot"></param>
    /// <param name="text"></param>
	void changeTextSlot(int numSlot, string text)
	{
		GameObject day_slot = GameObject.Find("Slot_"+numSlot);
		day_slot.GetComponent<Button>().GetComponentInChildren<Text>().text = text;
	}

    /// <summary>
    /// 获取这个item上代表的日期数
    /// </summary>
    /// <param name="numSlot"></param>
    /// <returns></returns>
	int getDayInSlot(int numSlot)
	{
		GameObject day_slot = GameObject.Find("Slot_"+ (numSlot));
		string txt = day_slot.GetComponentInChildren<Text>().text;
		return int.Parse(txt);
	}

    /// <summary>
    /// 标记选择的天数
    /// </summary>
    /// <param name="day"></param>
	public void markSelectionDay(int day)
	{
		GameObject day_slot = GameObject.Find("Slot_"+ (day+currentTime.dayOffset));

		// Change Image
		if(!checkEventExist(currentTime.year,currentTime.month,day))
		{
			Sprite sprite       = Resources.Load<Sprite>("img/circle_unfilled");
			day_slot.GetComponent<Image>().sprite   = sprite;
			day_slot.GetComponent<Image>().enabled  = true;
			day_slot.GetComponent<Image>().color    = FlatCalendarStyle.color_bubbleSelectionMarker;
			day_slot.GetComponent<Button>().GetComponentInChildren<Text>().color = FlatCalendarStyle.color_dayTextNormal;
		}
	
		// Update Text
		label_dayOfWeek.GetComponent<Text>().text = currentTime.dayOfWeek;
		label_dayNumber.GetComponent<Text>().text = "" + currentTime.day;
	}

    /// <summary>
    /// 取消选中的天数的圆圈
    /// </summary>
    /// <param name="day"></param>
	void unmarkSelctionDay(int day)
	{
		GameObject day_slot = GameObject.Find("Slot_"+ (day+currentTime.dayOffset));

		// Change Image
		if(!checkEventExist(currentTime.year,currentTime.month,day))
		{
			setNormalSlot(day+currentTime.dayOffset);
		}
	}
    /// <summary>
    /// 检查这一天有没有对应的事件
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <returns></returns>
	public static bool checkEventExist(int year, int month, int day)
	{
		if(events_list == null)
			return false;

		if(!events_list.ContainsKey(year))
			return false;

		if(!events_list[year].ContainsKey(month))
			return false;

		if(!events_list[year][month].ContainsKey(day))
			return false;

		if(events_list[year][month][day] == null)
			return false;

		if(events_list[year][month][day].Count == 0)
			return false;

		return true;
	}
    /// <summary>
    /// 添加按钮事件
    /// </summary>
	void addEventsListener()
	{
		btn_nextMonth.GetComponent<Button>().onClick.AddListener(() => evtListener_NextMonth());
		btn_prevMonth.GetComponent<Button>().onClick.AddListener(() => evtListener_PreviousMonth());
		btn_calendar.GetComponent<Button>().onClick.AddListener(()   => evtListener_GoToNowday());
		for(int i = 0; i < max_day_slots; i++)
			GameObject.Find("Slot_"+(i+1)).GetComponent<Button>().onClick.AddListener(() => evtListener_DaySelected());
	}

	public void setCurrentTime()
	{
		currentTime.year      = System.DateTime.Now.Year;
		currentTime.month     = System.DateTime.Now.Month;
		currentTime.day       = System.DateTime.Now.Day;
		currentTime.dayOfWeek = System.DateTime.Now.DayOfWeek.ToString();
		currentTime.totalDays = System.DateTime.DaysInMonth(currentTime.year,currentTime.month);
		currentTime.dayOffset = getIndexOfFirstSlotInMonth(currentTime.year,currentTime.month);
	}

	void setCurrentTime(FlatCalendar.TimeObj obj)
	{
		obj.year      = System.DateTime.Now.Year;
		obj.month     = System.DateTime.Now.Month;
		obj.day       = System.DateTime.Now.Day;
		obj.dayOfWeek = System.DateTime.Now.DayOfWeek.ToString();
		obj.totalDays = System.DateTime.DaysInMonth(obj.year,obj.month);
		obj.dayOffset = getIndexOfFirstSlotInMonth(obj.year,obj.month);
	}

	public void installDemoData()
	{
		addEvent(2016,3,7,  new EventObj("Event","Description"));
		addEvent(2016,3,7,  new EventObj("Event","Description"));
		addEvent(2016,3,10, new EventObj("Event","Description"));
		addEvent(2016,3,22, new EventObj("Event","Description"));
		addEvent(2016,4,5,  new EventObj("Event","Description"));
		addEvent(2016,4,5,  new EventObj("Event","Description"));
		addEvent(2016,4,5,  new EventObj("Event","Description"));
		addEvent(2016,4,15, new EventObj("Event","Description"));
		addEvent(2016,4,22, new EventObj("Event","Description"));
		addEvent(2016,5,1,  new EventObj("Event","Description"));
		addEvent(2016,5,2,  new EventObj("Event","Description"));
		addEvent(2016,5,3,  new EventObj("Event","Description"));
		addEvent(2016,5,15, new EventObj("Event","Description"));
		addEvent(2016,6,2,  new EventObj("Event","Description"));
		addEvent(2016,6,3,  new EventObj("Event","Description"));
		addEvent(2016,6,4,  new EventObj("Event","Description"));
		addEvent(2019,9,13, new EventObj("Event","Description"));

		//updateCalendar(currentTime.month,currentTime.year);
		//markSelectionDay(currentTime.day);
	}

	public void setUIStyle(int style)
	{
		current_UiStyle = style;
		FlatCalendarStyle.changeUIStyle(current_UiStyle);
	}

	public void addEvent(int year, int month, int day, EventObj ev)
	{
		if(!events_list.ContainsKey(year))
			events_list.Add(year,new Dictionary<int, Dictionary<int,List<EventObj>>>());
		
		if(!events_list[year].ContainsKey(month))
			events_list[year].Add(month,new Dictionary<int, List<EventObj>>());
		
		if(!events_list[year][month].ContainsKey(day))
			events_list[year][month].Add(day,new List<EventObj>());

		events_list[year][month][day].Add(ev);
	}

	public void removeEvent(int year, int month, int day, EventObj ev)
	{
		if(!events_list.ContainsKey(year))
			events_list.Add(year,new Dictionary<int, Dictionary<int,List<EventObj>>>());
		
		if(!events_list[year].ContainsKey(month))
			events_list[year].Add(month,new Dictionary<int, List<EventObj>>());
		
		if(!events_list[year][month].ContainsKey(day))
			events_list[year][month].Add(day,new List<EventObj>());

		if(events_list[year][month][day].Contains(ev))
			events_list[year][month][day].Remove(ev);
	}

	public void removeAllEventOfDay(int year, int month, int day)
	{
		if(!events_list.ContainsKey(year))
			events_list.Add(year,new Dictionary<int, Dictionary<int,List<EventObj>>>());
		
		if(!events_list[year].ContainsKey(month))
			events_list[year].Add(month,new Dictionary<int, List<EventObj>>());
		
		if(!events_list[year][month].ContainsKey(day))
			events_list[year][month].Add(day,new List<EventObj>());

		events_list[year][month][day].Clear();
	}

	public void removeAllCalendarEvents()
	{
		events_list.Clear();
	}

	public static List<EventObj> getEventList(int year, int month, int day)
	{
		List<EventObj> list = new List<EventObj>();

		if(!events_list.ContainsKey(year))
			return list;

		if(!events_list[year].ContainsKey(month))
			return list;

		if(!events_list[year][month].ContainsKey(day))
			return list;

		return events_list[year][month][day];
	}

	void updateUiLabelEvents(int year, int month, int day)
	{
		label_numberEvents.GetComponent<Text>().text = "" + getEventList(year,month,day).Count;
	}
	

	// ================================================
	// =============== BUTTON LISTENERS ===============
	// ================================================
	void evtListener_NextMonth()
	{
		unmarkSelctionDay(currentTime.day);

		currentTime.month = (currentTime.month+1) % 13;
		if(currentTime.month == 0)
		{
			currentTime.year++;
			currentTime.month = 1;
		}


		currentTime.day       = 1;
        currentTime.dayOfWeek = getDayOfWeek(currentTime.year,currentTime.month,currentTime.day);
		currentTime.dayOffset = getIndexOfFirstSlotInMonth(currentTime.year,currentTime.month);
        currentTime.totalDays = System.DateTime.DaysInMonth(currentTime.year, currentTime.month);
  

		updateCalendar(currentTime.month,currentTime.year);

		markSelectionDay(currentTime.day);

		// Update label event
		updateUiLabelEvents(currentTime.year,currentTime.month,currentTime.day);

		// Send Callback
		if(delegate_onmonthchanged != null)
			delegate_onmonthchanged(currentTime);
	}

	void evtListener_PreviousMonth()
	{
		unmarkSelctionDay(currentTime.day);

		currentTime.month = (currentTime.month-1) % 13;
		if(currentTime.month == 0)
		{
			currentTime.year--;
			currentTime.month = 12;
		}

		currentTime.day   = 1;
		currentTime.dayOfWeek = getDayOfWeek(currentTime.year,currentTime.month,currentTime.day);
		currentTime.dayOffset = getIndexOfFirstSlotInMonth(currentTime.year,currentTime.month);
        currentTime.totalDays = System.DateTime.DaysInMonth(currentTime.year, currentTime.month);

		updateCalendar(currentTime.month,currentTime.year);

		markSelectionDay(currentTime.day);

		// Update label event
		updateUiLabelEvents(currentTime.year,currentTime.month,currentTime.day);

		// Send Callback
		if(delegate_onmonthchanged != null)
			delegate_onmonthchanged(currentTime);

	}

	void evtListener_DaySelected()
	{
		// Unmark old slot
		unmarkSelctionDay(currentTime.day);

		// Update current day
		string slot_name             = EventSystem.current.currentSelectedGameObject.name;
		int    slot_position         = int.Parse(slot_name.Substring(5,(slot_name.Length-5)));
		 	   currentTime.day       = getDayInSlot(slot_position);
			   currentTime.dayOfWeek = getDayOfWeek(currentTime.year,currentTime.month,currentTime.day);

		// Mark current slot
		markSelectionDay(currentTime.day);

		// Update label event
		updateUiLabelEvents(currentTime.year,currentTime.month,currentTime.day);

		// Send Callback
		if(delegate_ondayselected != null)
			delegate_ondayselected(currentTime);

		// Send Callback
		if(getEventList(currentTime.year,currentTime.month,currentTime.day).Count > 0)
			if(delegate_oneventselected != null)
				delegate_oneventselected(currentTime,getEventList(currentTime.year,currentTime.month,currentTime.day));
	}

	void evtListener_GoToNowday()
	{
		// Unmark old slot
		unmarkSelctionDay(currentTime.day);

		// Set Current Time
		setCurrentTime();

		// Update Calendar
		updateCalendar(currentTime.month,currentTime.year);

		// Mark Selection Day
		markSelectionDay(currentTime.day);

		// Update label event
		updateUiLabelEvents(currentTime.year,currentTime.month,currentTime.day);

		// Send Callback
		if(delegate_onnowday != null)
			delegate_onnowday(currentTime);
	}

	// =========================================================
	// ================= SET DELEGATE CALLBACKS ================
	// =========================================================

	public void setCallback_OnDaySelected(Delegate_OnDaySelected func)
	{
		delegate_ondayselected = func;
	}

	public void setCallback_OnEventSelected(Delegate_OnEventSelected func)
	{
		delegate_oneventselected = func;
	}

	public void setCallback_OnMonthChanged(Delegate_OnMonthChanged func)
	{
		delegate_onmonthchanged = func;
	}

	public void setCallback_OnNowday(Delegate_OnNowDay func)
	{
		delegate_onnowday = func;
	}
}
