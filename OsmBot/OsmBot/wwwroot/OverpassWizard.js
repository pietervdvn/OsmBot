export default class OverpassWizard {

    prefix = "https://overpass-api.de/api/interpreter?data=";
    scriptStart = "%5Bout%3Ajson%5D%5Btimeout%3A25%5D%3B%28";
    scriptEnd = "%29%3Bout%20body%3B%3E%3Bout%20skel%20qt%3B";
    cache = {};

    AsQuery(query) {
        return Object.keys(query).map(key => {
                if (query[key] === "*") {
                    return "%5B%22" + key + "%22%5D";
                }
                return "%5B%22" + key + "%22%3D%22" + query[key] + "%22%5D";
            }
        ).join("");
    }

    /*
    * typ: one of 'node', 'way', 'relation' or 'nwr'
    * query: an object with key-value pairs which should all be matched (AND)
    * callback: the geojson data will be pushed into this
    * */
    constructor(typ, callback, query, queryOr) {

        this.queries = [typ + this.AsQuery(query)]
        if (queryOr !== undefined) {
            this.queries.push(typ + this.AsQuery(queryOr));
        }
        this.callback = callback;
        this.lastRun = undefined;
        this.oldMinLat = 0;
        this.oldMaxLat = 0;
        this.oldMinLon = 0;
        this.oldMaxLon = 0;
        this.queryRunning = false;
    }

    RunQuery(lon0, lon1, lat0, lat1) {
        
        if(this.queryRunning && (new Date() - this.lastRun) <  15000 ){
            console.log("A query is already running, please be patient");
            return;
        }

        let minLat = Math.min(lat0, lat1);
        let maxLat = Math.max(lat0, lat1);
        let minLon = Math.min(lon0, lon1);
        let maxLon = Math.max(lon0, lon1);
        
        if(this.oldMinLat <= minLat && maxLat <= this.oldMaxLat  &&
            this.oldMinLon <= minLon && maxLon <= this.oldMaxLon){
            console.log("Loaded already contain the given data");
            return;
        }
        
        if((new Date() - this.lastRun) < 1000 ){
            console.log("Too soon", new Date() - this.lastRun);
            return;
        }

   
        
        
        
        this.lastRun = new Date();
        
        minLon = minLon - 0.05;
        maxLon = maxLon + 0.05;
        minLat = minLat - 0.05;
        maxLat = maxLat + 0.05;
        
        let query = this.GenQuery(minLon, maxLon, minLat, maxLat);
        console.log("Running query");

        if (this.cache[query] !== undefined) {
            this.callback(this.cache[query]);
            return;
        }

        const Http = new XMLHttpRequest();
        Http.open("GET", query);
        Http.send();

        Http.onreadystatechange = (e) => {
            this.queryRunning = false;
            if (Http.responseText === "") {
                return;
            }
            try {
                let geojson = osmtogeojson(JSON.parse(Http.responseText));
                this.cache[query] = geojson;
                this.oldMinLat = minLat;
                this.oldMaxLat = maxLat;
                this.oldMinLon = minLon;
                this.oldMaxLon = maxLon;
                this.callback(geojson);
            } catch (e) {
                // POKEMON!
                console.log(query, e)
            }
        }

    }

    GenQuery(lon0, lon1, lat0, lat1) {
        let query = "";
        for (var i in this.queries) {
            let qi = this.queries[i];
            query += qi + this.GenBBox(lon0, lon1, lat0, lat1);
        }
        return this.prefix + this.scriptStart + query + this.scriptEnd;
    }

    GenBBox(lon0, lon1, lat0, lat1) {
        let minLat = Math.min(lat0, lat1);
        let maxLat = Math.max(lat0, lat1);
        let minLon = Math.min(lon0, lon1);
        let maxLon = Math.max(lon0, lon1);
        return "%28" + minLat + "%2C" + minLon + "%2C" + maxLat + "%2C" + maxLon + "%29%3B";
    }


}