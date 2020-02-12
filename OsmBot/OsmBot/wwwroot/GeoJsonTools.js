export default class GeoJsonTools{
    
    static Sum(list){
        var s = 0;
        for (var i=0; i < list.length; i++) {
            let v = list[i];
            s += v;
        }
        return s;
    }
    
    static Average(list){
        return this.Sum(list) / list.length
    }
    
    static wayToPoint(feature){
        if(feature.geometry.type === "Point"){
            return feature;
        }
        let lats = feature.geometry.coordinates.map(el => el[0]);
        let lons = feature.geometry.coordinates.map(el => el[1]);
        let coor =
            [GeoJsonTools.Average(lats), GeoJsonTools.Average(lons)];
        return {
            type: feature.type,
            properties: feature.properties,
            geometry: {
                type: "Point",
                coordinates: coor
            }

        };
    }
    
    static WayToPoint(geojson){
        var features = geojson.features.map(GeoJsonTools.featureToPoint);
        var newGeoJson = {
            type: geojson.type,
            generator: geojson.generator,
            timestamp: geojson.timestamp,
            copyright: geojson.copyright,
            features: features};
        return newGeoJson;
    }
    
    
}