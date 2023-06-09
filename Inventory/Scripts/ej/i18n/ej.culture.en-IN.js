ej.addCulture( "en-IN", {
	name: "en-IN",
	englishName: "English (India)",
	nativeName: "English (India)",
	language: 'en',
	numberFormat: {
		pattern: ['-n'],
		groupSizes: [3,2,0],
		',': ',',
		'.': '.',
		percent: {
			pattern: ["-n%","n%"],
			groupSizes: [3,2,0],
			',': ',',
			'.': '.',
			symbol: '%'
		},
		currency: {
			pattern: ["$ -n","$ n"],
			groupSizes: [3,2,0],
			',': ',',
			'.': '.',
			symbol: "₹"
		}
	},
	calendars: {
		standard: {
			"/": "-",
			firstDay: 1,
			days: {
	            names: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"],
	            namesAbbr: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"],
	            namesShort: ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"]
	        },
	        months: {
	            names: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December", ""],
	            namesAbbr: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", ""]
	        },
	        AM: ['AM', 'am', 'AM'],
	        PM: ['PM', 'pm', 'PM'],
			patterns: {
				d: "dd/MM/yyyy",
				D: "dd MMMM yyyy",
				t: "HH:mm",
				T: "HH:mm:ss",
				f: "dd MMMM yyyy HH:mm",
				F: "dd MMMM yyyy HH:mm:ss",
				M: "d MMMM",
				Y: "MMMM, yyyy"
			}
		}
	}
});