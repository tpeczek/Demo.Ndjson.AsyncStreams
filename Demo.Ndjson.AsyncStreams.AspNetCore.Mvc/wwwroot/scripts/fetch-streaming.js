const FetchStreaming = (function () {

    let weatherForecastsTable;
    let fetchWeatherForecastsJsonButton, fetchWeatherForecastsJsonStreamButton, fetchWeatherForecastsNdjsonStreamButton, postWeatherForecastsNdjsonStreamButton;

    function initializeUI() {
        fetchWeatherForecastsJsonButton = document.getElementById('fetch-weather-forecasts-json');
        fetchWeatherForecastsJsonButton.addEventListener('click', fetchWeatherForecastsJson);

        fetchWeatherForecastsJsonStreamButton = document.getElementById('fetch-weather-forecasts-json-stream');
        fetchWeatherForecastsJsonStreamButton.addEventListener('click', fetchWeatherForecastsJsonStream);

        fetchWeatherForecastsNdjsonStreamButton = document.getElementById('fetch-weather-forecasts-ndjson-stream');
        fetchWeatherForecastsNdjsonStreamButton.addEventListener('click', fetchWeatherForecastsNdjsonStream);

        postWeatherForecastsNdjsonStreamButton = document.getElementById('post-weather-forecasts-ndjson-stream');
        postWeatherForecastsNdjsonStreamButton.addEventListener('click', postWeatherForecastsNdjsonStream);
        
        weatherForecastsTable = document.getElementById('weather-forecasts');
    };

    function fetchWeatherForecastsJson() {
        clearWeatherForecasts();

        fetch('api/WeatherForecasts')
            .then(function (response) {
                return response.json();
            })
            .then(function (weatherForecasts) {
                weatherForecasts.forEach(appendWeatherForecast);
            });
    };

    function fetchWeatherForecastsJsonStream() {
        clearWeatherForecasts();

        oboe('api/WeatherForecasts/negotiate-stream')
            .node('!.*', function (weatherForecast) {
                appendWeatherForecast(weatherForecast);
            });
    }

    function fetchWeatherForecastsNdjsonStream() {
        clearWeatherForecasts();

        fetch('api/WeatherForecasts/stream')
            .then(function (response) {
                const weatherForecasts = response.body
                    .pipeThrough(new TextDecoderStream())
                    .pipeThrough(parseNDJSON());

                readWeatherForecastsStream(weatherForecasts.getReader());
            });
    };

    function postWeatherForecastsNdjsonStream() {
        const weatherForecastsStream = WeatherForecaster.getWeatherForecastsStream().pipeThrough(new TextEncoderStream());

        fetch('api/WeatherForecasts/stream', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-ndjson' },
            body: weatherForecastsStream,
            duplex: 'half'
        });
    };

    function clearWeatherForecasts() {
        for (let rowIndex = 1; rowIndex  < weatherForecastsTable.rows.length;) {
            weatherForecastsTable.deleteRow(rowIndex );
        }
    };

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
    };

    function readWeatherForecastsStream(weatherForecastsStreamReader) {
        weatherForecastsStreamReader.read()
            .then(function (result) {
                if (!result.done) {
                    appendWeatherForecast(result.value);

                    readWeatherForecastsStream(weatherForecastsStreamReader);
                }
            });
    };

    function appendWeatherForecast(weatherForecast) {
        let weatherForecastRow = weatherForecastsTable.insertRow(-1);

        weatherForecastRow.insertCell(0).appendChild(document.createTextNode(weatherForecast.dateFormatted));
        weatherForecastRow.insertCell(1).appendChild(document.createTextNode(weatherForecast.temperatureC));
        weatherForecastRow.insertCell(2).appendChild(document.createTextNode(weatherForecast.temperatureF));
        weatherForecastRow.insertCell(3).appendChild(document.createTextNode(weatherForecast.summary));
    };

    return {
        initialize: function () {
            initializeUI();
        }
    };
})();

FetchStreaming.initialize();