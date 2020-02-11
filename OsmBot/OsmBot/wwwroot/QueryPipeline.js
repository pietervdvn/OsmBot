export default class QueryPipeline {

    minZoom = 10;

    constructor(mapObject, overpasser, minZoom, callback) {
        this.minZoom = minZoom;
        console.log(mapObject)
        this.mapObject = mapObject;
        console.log(this.mapObject)
        mapObject.on("moveend", this.Run())
        overpasser.callback = callback;
        this.overpasser = overpasser;
        this.Run = this.Run.bind(this)
        this.callback = callback;
    }

    Run() {
        // Wrapping this in a lambda shouldn't be necessary... but it is
        return (e) => {
            if (this.mapObject.getZoom() < this.minZoom) {
                console.log("Zoom in more to see the data");
                return;
            }

            var bounds = this.mapObject.getBounds();
            this.overpasser.RunQuery(bounds._northEast.lng, bounds._southWest.lng,
                bounds._northEast.lat, bounds._southWest.lat);

        }
    }


}