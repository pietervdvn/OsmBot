var geoJsonLayer;

function addGeoJsonTo(map, f) {
    return (data) => {
        if (geoJsonLayer !== undefined) {
            geoJsonLayer.clearLayers();
        }
        geoJsonLayer = L.geoJSON(data, {
            onEachFeature: f
        });
        geoJsonLayer.addTo(map);
    }
}

function initializeMap() {

    // load the tile layer from GEO6
    var wmsLayer = L.tileLayer.wms('https://geoservices.informatievlaanderen.be/raadpleegdiensten/OGW/wms?s',
        {
            layers: "OGWRGB13_15VL",
            attribution: "Luchtfoto's van © AIV Vlaanderen (2013-2015) | Data van OpenStreetMap"
        });

    var osmLayer = L.tileLayer("https://b.tile.openstreetmap.org/{z}/{x}/{y}.png",
        {
            attribution: 'Map Data and background © <a href="osm.org">OpenStreetMap</a>',
            maxZoom: 21,
            minZoom: 1
        });
    var osmBeLayer = L.tileLayer("https://tile.osm.be/osmbe/{z}/{x}/{y}.png",
        {
            attribution: 'Map Data and background © <a href="osm.org">OpenStreetMap</a> | <a href="https://geo6.be/">Tiles courtesy of Geo6</a>',
            maxZoom: 21,
            minZoom: 1
        });

    var grbLayer = L.tileLayer("https://tile.informatievlaanderen.be/ws/raadpleegdiensten/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=grb_bsk&STYLE=&FORMAT=image/png&tileMatrixSet=GoogleMapsVL&tileMatrix={z}&tileCol={x}&tileRow={y}",
        {
            attribution: 'Map Data   <a href="osm.org">OpenStreetMap</a> | Background <i>Grootschalig ReferentieBestand</i>(GRB) © AGIV',
            maxZoom: 21,
            minZoom: 1,
            wmts: true
        });


    let defaultLayer = osmBeLayer;
    var baseLayers = {
        "OpenStreetMap Be": osmBeLayer,
        "OpenStreetMap": osmLayer,
        "Luchtfoto AIV Vlaanderen": wmsLayer,
        "GRB Vlaanderen": grbLayer
    };

    var urlParams = new URLSearchParams(window.location.search);
    let selectedLayerName = urlParams.get('layer')

    if (baseLayers[selectedLayerName]) {
        defaultLayer = baseLayers[selectedLayerName];
    }

    map = L.map('map', {
        center: [50.9, 3.9],
        zoom: 9,
        layers: [defaultLayer]
    });

    L.control.layers(baseLayers).addTo(map);

    return map;
}