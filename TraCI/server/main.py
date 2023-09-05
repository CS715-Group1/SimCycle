from flask import Flask
from sim import sim_bp
from vehicle import vehicle_bp

app = Flask(__name__)

# Routes
app.register_blueprint(sim_bp, url_prefix='/sim')
app.register_blueprint(vehicle_bp, url_prefix='/vehicle')


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5454)
