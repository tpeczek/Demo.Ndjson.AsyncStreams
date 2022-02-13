const FetchStreaming = (function () {

    let abortController;
    let weatherForecastsTable;
    let fetchWeatherForecastsButton, streamWeatherForecastsNdjsonButton, streamWeatherForecastsJsonButton, abortButton;

    function initializeUI() {
        fetchWeatherForecastsButton = document.getElementById('fetch-weather-forecasts');
        fetchWeatherForecastsButton.addEventListener('click', fetchWeatherForecasts);

        streamWeatherForecastsNdjsonButton = document.getElementById('stream-weather-forecasts-ndjson');
        streamWeatherForecastsNdjsonButton.addEventListener('click', streamWeatherForecastsNdjson);

        streamWeatherForecastsJsonButton = document.getElementById('stream-weather-forecasts-json');
        streamWeatherForecastsJsonButton.addEventListener('click', streamWeatherForecastsJson);

        abortButton = document.getElementById('abort');
        abortButton.addEventListener('click', triggerAbortSignal);

        weatherForecastsTable = document.getElementById('weather-forecasts');
    }

    function fetchWeatherForecasts() {
        abortController = new AbortController();

        switchButtonsState(true);
        clearWeatherForecasts();

        fetch('api/WeatherForecasts', { signal: abortController.signal })
            .then(function (response) {
                return response.json();
            })
            .then(function (weatherForecasts) {
                weatherForecasts.forEach(appendWeatherForecast);
                switchButtonsState(false);
            });
    }

    function streamWeatherForecastsNdjson() {
        abortController = new AbortController();

        switchButtonsState(true);
        clearWeatherForecasts();

        fetch('api/WeatherForecasts/negotiate-stream', { headers: { 'Accept': 'application/x-ndjson' }, signal: abortController.signal })
            .then(function (response) {
                const weatherForecasts = response.body
                    .pipeThrough(new TextDecoderStream())
                    .pipeThrough(transformWeatherForecastsNdjsonStream());

                readWeatherForecastsNdjsonStream(weatherForecasts.getReader());
            });
    }

    function streamWeatherForecastsJson() {
        abortController = new AbortController();
        const abortSignal = abortController.signal;

        switchButtonsState(true);
        clearWeatherForecasts();

        const oboeInstance = oboe('api/WeatherForecasts/negotiate-stream')
            .node('!.*', function (weatherForecast) {
                appendWeatherForecast(weatherForecast);
            })
            .done(function () {
                switchButtonsState(false);
            });

        abortSignal.onabort = function () {
            oboeInstance.abort();
        };
    }

    function triggerAbortSignal() {
        if (abortController) {
            abortController.abort();
            switchButtonsState(false);
        }
    }

    function switchButtonsState(operationInProgress) {
        fetchWeatherForecastsButton.disabled = operationInProgress;
        streamWeatherForecastsNdjsonButton.disabled = operationInProgress;
        streamWeatherForecastsJsonButton.disabled = operationInProgress;
        abortButton.disabled = !operationInProgress;
    }

    function clearWeatherForecasts() {
        for (let rowIndex = 1; rowIndex  < weatherForecastsTable.rows.length;) {
            weatherForecastsTable.deleteRow(rowIndex );
        }
    }

    function transformWeatherForecastsNdjsonStream() {
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

    function readWeatherForecastsNdjsonStream(weatherForecastsStreamReader) {
        weatherForecastsStreamReader.read()
            .then(function (result) {
                if (!result.done) {
                    appendWeatherForecast(result.value);

                    readWeatherForecastsNdjsonStream(weatherForecastsStreamReader);
                } else {
                    switchButtonsState(false);
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