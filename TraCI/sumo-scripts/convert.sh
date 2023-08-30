netconvert --xml-type-files ${SUMO_HOME}/data/typemap/osmNetconvert.typ.xml,${SUMO_HOME}/data/typemap/osmNetconvertUrbanDe.typ.xml \
  --osm-files ../Sumo/Data/uoa.osm --output-file ../Sumo/Data/uoa.net.xml \
  --geometry.remove --roundabouts.guess --ramps.guess \
  --junctions.join --tls.guess-signals --tls.discard-simple --tls.join