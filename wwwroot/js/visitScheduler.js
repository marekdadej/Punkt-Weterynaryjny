function initVisitScheduler(dateInputId, hoursListId, visitDateId, selectedTime = null, fullDateTime = null) {
    const dateInput = document.getElementById(dateInputId);
    const hoursList = document.getElementById(hoursListId);
    const visitInput = document.getElementById(visitDateId);

    if (!dateInput || !hoursList || !visitInput) {
        console.error("Brak wymaganych elementów na stronie.");
        return;
    }

    flatpickr(dateInput, {
        dateFormat: "Y-m-d",
        allowInput: false,
        minDate: "today",
        onChange: function (selectedDates, dateStr) {
            dateInput.value = dateStr;
            visitInput.value = "";

            fetch(`/Visits/GetAvailableHours?date=${dateStr}`)
                .then(res => res.json())
                .then(hours => {
                    hoursList.innerHTML = "";

                    if (hours.length === 0) {
                        hoursList.innerHTML = "<div class='text-danger'>Brak dostępnych godzin</div>";
                        return;
                    }

                    hours.forEach(hour => {
                        const btn = document.createElement("button");
                        btn.type = "button";
                        btn.textContent = hour;
                        btn.className = "btn btn-outline-primary m-1";
                        btn.onclick = () => {
                            visitInput.value = dateStr + "T" + hour;
                            document.querySelectorAll(`#${hoursListId} button`).forEach(b => b.classList.remove("active"));
                            btn.classList.add("active");
                        };
                        hoursList.appendChild(btn);
                    });
                });
        }
    });

    if (fullDateTime) {
        const datePart = fullDateTime.substring(0, 10);
        const timePart = fullDateTime.substring(11, 16);
        dateInput.value = datePart;
        visitInput.value = fullDateTime;

        fetch(`/Visits/GetAvailableHours?date=${datePart}`)
            .then(res => res.json())
            .then(hours => {
                hoursList.innerHTML = "";

                if (hours.length === 0) {
                    hoursList.innerHTML = "<div class='text-danger'>Brak dostępnych godzin</div>";
                    return;
                }

                hours.forEach(hour => {
                    const btn = document.createElement("button");
                    btn.type = "button";
                    btn.textContent = hour;
                    btn.className = "btn btn-outline-primary m-1";
                    if (hour === timePart) {
                        btn.classList.add("active");
                    }
                    btn.onclick = () => {
                        visitInput.value = datePart + "T" + hour;
                        document.querySelectorAll(`#${hoursListId} button`).forEach(b => b.classList.remove("active"));
                        btn.classList.add("active");
                    };
                    hoursList.appendChild(btn);
                });
            });
    }
}
