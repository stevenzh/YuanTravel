jQuery.extend(jQuery.jtsage.datebox.prototype.options.lang, {
    'zh-CN': {
        setDateButtonLabel: "设置",
        setTimeButtonLabel: "Set Time",
        setDurationButtonLabel: "Set Duration",
        todayButtonLabel: "Jump to Today",
        titleDateDialogLabel: "日期设置",
        titleTimeDialogLabel: "Set Time",
        daysOfWeek: ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"],
        daysOfWeekShort: ["日", "一", "二", "三", "四", "五", "六"],
        monthsOfYear: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"],
        monthsOfYearShort: ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"],
        durationLabel: ["天", "小时", "分钟", "秒"],
        durationDays: ["Day", "Days"],
        tooltip: "Open Date Picker",
        nextMonth: "Next Month",
        prevMonth: "Previous Month",
        timeFormat: 12,
        headerFormat: "%Y, %B %-d, %A",
        dateFieldOrder: ["y", "m", "d"],
        timeFieldOrder: ["h", "i", "a"],
        slideFieldOrder: ["y", "m", "d"],
        datetimeFieldOrder: ["y", "m", "d", "h", "i", "s", "a"],
        dateFormat: "%Y-%m-%d",
        datetimeFormat: "%Y-%m-%dT%k:%M:%S",
        useArabicIndic: false,
        isRTL: false,
        calStartDay: 0,
        clearButton: "清除",
        durationOrder: ["d", "h", "i", "s"],
        meridiem: ["AM", "PM"],
        timeOutput: "%k:%M", // 12hr: "%l:%M %p", 24hr: "%k:%M",
        durationFormat: "%Dd %DA, %Dl:%DM:%DS",
        calDateListLabel: "其他日期",
        calHeaderFormat: "%B %Y",
        tomorrowButtonLabel: "跳转到明天",
        useButton:false
    }
});
jQuery.extend(jQuery.jtsage.datebox.prototype.options, {
    useLang: 'zh-CN'
});