from flatlib.chart import Chart
from flatlib.datetime import Datetime
from flatlib.geopos import GeoPos

date = Datetime('2000/01/01', '12:00', '+04:00')
pos = GeoPos(40.37, 49.82)

chart = Chart(date, pos)

print(chart.get('Sun'))
print("flatlib işləyir!")
