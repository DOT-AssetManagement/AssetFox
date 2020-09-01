const mongoose = require('mongoose');
const debug = require('debug')('app:mongoose');

module.exports = function(config) {
    //mongoose.createConnection(config.db, { useNewUrlParser: true, useUnifiedTopology: true })
    mongoose.connect(config.db, { useNewUrlParser: true, useUnifiedTopology: true })
    .then(() => {
      debug('connected to mongo db at ' + config.db);
    })
    .catch((err) => {
      debug('error has occured in connection: ' + err);
    });
    debug('DB connection props: ' + config.db)
};
