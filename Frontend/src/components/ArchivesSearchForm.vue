<template>
  <div>
    <form @submit.stop.prevent="search">
        <div class="form-inline">
          <input type="text" class="col-md-1 form-control form-control-sm" v-model="searchProtocol.keyword" placeholder="제목 (like 검색)">
          <select class="col-md-1 form-control form-control-sm" v-model="searchProtocol.type" name="type">
            <option :value=null>타입 (전체)</option>
            <option v-for="(value, key) in ARCHIVES_TYPES" :value="key" :key="value.text">{{value.text}}</option>
          </select>
          <select class="col-md-1 form-control form-control-sm" @change="changeLimit()" v-model="limit" name="limit">
            <option v-for="(value) in LIMIT_TYPES" :value="value" :key="value">{{value}}</option>
          </select>
          <button type="submit" class="btn btn-sm btn-primary">검색<i class="fa fa-sm fa-search"></i></button>&nbsp;
          <button type="button" class="btn btn-sm btn-secondary" @click.prevent="reset">초기화 <i class="fa fa-sm fa-eraser"></i></button>
        </div>
    </form>
  </div>
</template>

<script>
import _ from 'lodash'
import {
  ARCHIVES_TYPES,
  LIMIT_TYPES
} from '@/common/constant/types'

const SEARCH_PROTOCOL = {
  type: null,
  keyword: null
}
Object.freeze(SEARCH_PROTOCOL)

export default {
  data () {
    return {
      searchProtocol: Object.assign({}, SEARCH_PROTOCOL),
      ARCHIVES_TYPES: ARCHIVES_TYPES,
      LIMIT_TYPES: LIMIT_TYPES,
      limit: LIMIT_TYPES[0]
    }
  },
  beforeMount () {
    var searchProtocol = JSON.parse(this.$localStorage.get('searchProtocol'))
    if (searchProtocol === null) {
      this.reset()
    } else {
      if (_.isEqual(Object.keys(searchProtocol), Object.keys(SEARCH_PROTOCOL))) {
        this.searchProtocol = searchProtocol
        this.$emit('searching', this.searchProtocol)
      } else {
        this.reset()
      }
    }
  },
  methods: {
    search (e) {
      this.$localStorage.set('searchProtocol', JSON.stringify(this.searchProtocol))
      this.$emit('searching', this.searchProtocol)
    },
    changeLimit () {
      this.$emit('changeLimit', this.limit)
    },
    reset () {
      this.searchProtocol = Object.assign({}, SEARCH_PROTOCOL)
      this.limit = LIMIT_TYPES[0]
      this.$localStorage.set('searchProtocol', null)
      this.$emit('searching', this.searchProtocol)
    }
  }
}
</script>

<style>
.form-control {
  margin-right: 3px;
  margin-bottom: 5px;
}

.form-inline {
  margin-bottom: 5px;
}

</style>
