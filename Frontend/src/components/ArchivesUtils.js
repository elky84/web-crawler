import moment from 'moment'
import abbreviate from 'number-abbreviate'

const ArchivesUtils = {
  momentTime (date) {
    var format = 'YYYY-MM-DD'
    let today = moment().format(format)
    let archiveDate = moment(date).format(format)

    if (today === archiveDate) {
      return moment(date).format('HH:mm:ss')
    } else {
      return moment(date).format('YYYY-MM-DD')
    }
  },
  abbreviation (number) {
    return abbreviate(number)
  },
  substr (str) {
    if (str.length > 30) {
      return str.substr(0, 30) + '...'
    }
    return str
  }
}

export default ArchivesUtils
