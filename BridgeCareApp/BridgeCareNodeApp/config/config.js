module.exports = {
  development: {
    db: 'mongodb://admin:BridgecareARA123@localhost:27017/BridgeCare',
    //db: 'mongodb://nbis_admin:BridgecareARA123@52.188.172.73:27017/NBIS_DEMO',
    port: process.env.PORT || 5000
  },
  production: {
    db: 'mongodb://nbis_admin:BridgecareARA123@localhost:27017/NBIS_DEMO',
    //db: 'mongodb://admin:BridgecareARA123@52.188.172.73:27017/BridgeCare?replicaSet=r1',
    port: process.env.PORT || 80
  }
}