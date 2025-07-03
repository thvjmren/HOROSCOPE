from flask import Flask, request, jsonify
from flatlib.chart import Chart
from flatlib.datetime import Datetime
from flatlib.geopos import GeoPos
from flatlib.const import ASC
import traceback

app = Flask(__name__)

def degree_to_sign(degree):
    signs = [
        "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo",
        "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces"
    ]
    index = int(degree // 30)
    return signs[index]

@app.route('/api/astro', methods=['GET'])
def astro():
    date = request.args.get('date')
    time = request.args.get('time')
    lat = request.args.get('lat')
    lon = request.args.get('lon')

    if not all([date, time, lat, lon]):
        return jsonify({"error": "Missing required parameters"}), 400

    try:
        lat = float(lat.replace(',', '.'))
        lon = float(lon.replace(',', '.'))
    except ValueError:
        return jsonify({"error": "Invalid latitude or longitude"}), 400

    date = date.replace('-', '/')

    try:
        if len(time.split(':')) == 2:
            time = time + ':00'

        dt = Datetime(date, time, '+00:00')
        pos = GeoPos(lat, lon)
        chart = Chart(dt, pos)

        sun = chart.get('Sun').sign
        moon = chart.get('Moon').sign

        asc_obj = chart.get(ASC)
        if asc_obj is None:
            return jsonify({"error": "Ascendant not found"}), 500

        asc = asc_obj.sign

        asc_degree = asc_obj.lon
        dsc_degree = (asc_degree + 180) % 360
        dsc = degree_to_sign(dsc_degree)

        result = {
            "Sun": sun,
            "Moon": moon,
            "Ascendant": asc,
        }

        return jsonify(result)

    except Exception as e:
        tb = traceback.format_exc()
        return jsonify({"error": str(e), "traceback": tb}), 500

if __name__ == '__main__':
    app.run(debug=True)
