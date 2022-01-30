﻿const FetchStreaming = (function () {

    let weatherForecastsTable;
    let fetchButton, fetchStreamButton, postWeatherForecastsNdjsonButton;

    function initializeUI() {
        fetchButton = document.getElementById('fetch');
        fetchButton.addEventListener('click', fetchWeatherForecasts);

        fetchStreamButton = document.getElementById('fetch-stream');
        fetchStreamButton.addEventListener('click', fetchWeatherForecastsStream);

        postWeatherForecastsNdjsonButton = document.getElementById('post-weather-forecasts-ndjson');
        postWeatherForecastsNdjsonButton.addEventListener('click', postWeatherForecastsNdjson);

        weatherForecastsTable = document.getElementById('weather-forecasts');
    };

    function fetchWeatherForecasts() {
        clearWeatherForecasts();

        fetch('api/WeatherForecasts')
            .then(function (response) {
                return response.json();
            })
            .then(function (weatherForecasts) {
                weatherForecasts.forEach(appendWeatherForecast);
            });
    };

    function fetchWeatherForecastsStream() {
        clearWeatherForecasts();

        fetchWeatherForecastsNdjson('api/WeatherForecasts/stream');
    };

    function fetchWeatherForecastsNdjson(route) {
        clearWeatherForecasts();

        fetch(route)
            .then(function (response) {
                const weatherForecasts = response.body
                    .pipeThrough(new TextDecoderStream())
                    .pipeThrough(parseNDJSON());

                readWeatherForecastsStream(weatherForecasts.getReader());
            });
    };

    function postWeatherForecastsNdjson() {
        const weatherForecastsStream = WeatherForecaster.getWeatherForecastsStream().pipeThrough(new TextEncoderStream());

        fetch('api/WeatherForecasts/stream', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-ndjson' },
            body: weatherForecastsStream
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