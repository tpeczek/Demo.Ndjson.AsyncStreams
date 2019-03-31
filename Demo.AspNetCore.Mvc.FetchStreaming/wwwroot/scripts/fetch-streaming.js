const FetchStreaming = (function () {

    let weatherForecastsTable;
    let fetchButton, fetchStreamButton;

    function initializeUI() {
        fetchButton = document.getElementById('fetch');
        fetchButton.addEventListener('click', fetchWeatherForecasts);

        fetchStreamButton = document.getElementById('fetch-stream');
        fetchStreamButton.addEventListener('click', fetchWeatherForecastsStream);

        weatherForecastsTable = document.getElementById('weather-forecasts');
    }

    function fetchWeatherForecasts() {
        clearWeatherForecasts();

        fetch('api/WeatherForecasts')
            .then(function (response) {
                return response.json();
            })
            .then(function (weatherForecasts) {
                weatherForecasts.forEach(appendWeatherForecast);
            });
    }

    function fetchWeatherForecastsStream() {
        clearWeatherForecasts();

        fetch('api/WeatherForecasts/stream')
            .then(function (response) {
                const weatherForecasts = response.body
                    .pipeThrough(new TextDecoderStream())
                    .pipeThrough(parseNDJSON());

                readWeatherForecastsStream(weatherForecasts.getReader());
            });
    }

    function clearWeatherForecasts() {
        for (let rowIndex = 1; rowIndex  < weatherForecastsTable.rows.length;) {
            weatherForecastsTable.deleteRow(rowIndex );
        }
    }

    function parseNDJSON() {
        let ndjsonBuffer = '';

        return new TransformStream({
            transform: function(ndjsonChunk, controller) {
                ndjsonBuffer += ndjsonChunk;

                const jsonValues = ndjsonBuffer.split('\n');
                jsonValues.slice(0, -1).forEach(function (jsonValue) { controller.enqueue(JSON.parse(jsonValue)); });

                ndjsonBuffer = jsonValues[jsonValues.length - 1];
            },
            flush: function(controller) {
                if (ndjsonBuffer) {
                    controller.enqueue(JSON.parse(ndjsonBuffer));
                }
            }
        });
    }

    function readWeatherForecastsStream(weatherForecastsStreamReader) {
        weatherForecastsStreamReader.read()
            .then(function (result) {
                if (!result.done) {
                    appendWeatherForecast(result.value);

                    readWeatherForecastsStream(weatherForecastsStreamReader);
                }
            });
    }

    function appendWeatherForecast(weatherForecast) {
        let weatherForecastRow = weatherForecastsTable.insertRow(-1);

        weatherForecastRow.insertCell(0).appendChild(document.createTextNode(weatherForecast.dateFormatted));
        weatherForecastRow.insertCell(1).appendChild(document.createTextNode(weatherForecast.temperatureC));
        weatherForecastRow.insertCell(2).appendChild(document.createTextNode(weatherForecast.temperatureF));
        weatherForecastRow.insertCell(3).appendChild(document.createTextNode(weatherForecast.summary));
    }

    return {
        initialize: function () {
            initializeUI();
        }
    };
})();

FetchStreaming.initialize();