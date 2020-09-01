module.exports = {
  development: {
    db: 'mongodb://admin:BridgecareARA123@52.188.172.73:27017/BridgeCare',
    port: process.env.PORT || 5000
  },
  production: {
    db: 'mongodb://admin:BridgecareARA123@52.188.172.73:27017/BridgeCare',
    //db: 'mongodb://admin:BridgecareARA123@52.188.172.73:27017/BridgeCare?replicaSet=r1',
    port: process.env.PORT || 80
  }
}