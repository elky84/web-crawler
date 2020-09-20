<template>
  <div class="container-fluid">
    <archives-searchForm @searching="parentSearching(... arguments)" @changeLimit="changeLimit(... arguments)" ref="searchForm"/>

    <table class="table table-bordered">
      <thead class="thead-dark">
        <tr class="text-center">
          <th width="120px" v-on:click="sortBy('Type')">
            <span class="header">사이트</span>
            <span class="arrow" :class="toArrow('Type')"/>
          </th>
          <th width="120px" v-on:click="sortBy('BoardName')">
            <span class="header">게시판</span>
            <span class="arrow" :class="toArrow('BoardName')"/>
          </th>
          <th width="120px" v-on:click="sortBy('Category')">
            <span class="header">카테고리</span>
            <span class="arrow" :class="toArrow('Category')"/>
          </th>
          <th v-on:click="sortBy('Title')">
            <span class="header">글 제목</span>
            <span class="arrow" :class="toArrow('Title')"/>
          </th>
          <th width="150px" v-on:click="sortBy('DateTime')">
            <span class="header">시간</span>
            <span class="arrow" :class="toArrow('DateTime')"/>
          </th>
          <th width="70px" v-on:click="sortBy('Count')">
            <span class="header">읽음</span>
            <span class="arrow" :class="toArrow('Count')"/>
          </th>
        </tr>
      </thead>
      <tbody>
        <template v-for="(archive, index) in archives">
          <tr class="cursor-pointer" :key="archive._id" @click.prevent="onClickLink(archive, index)">
            <td align="center"><span class="type">{{archive.type}}</span></td>
            <td align="center">
              <span class="badge block-badge" v-bind:class=ARCHIVES_TYPES[archive.type].label>
                {{archive.boardName}}
              </span>
            </td>
            <td align="center"><span class="category">{{archive.category}}</span></td>
            <td align="left"><span class="title"><a v-bind:href=archive.href>{{archive.title}}</a></span></td>
            <td align="center"><span class="time">{{momentTime(archive.dateTime)}}</span></td>
            <td align="center"><span class="count">{{abbreviation(archive.count)}}</span></td>
          </tr>
        </template>
      </tbody>
    </table>

    <b-pagination align="center" size="md" v-model="currentPage" :limit="20" :total-rows="totalItems" :per-page="limit" @change="listing(... arguments)" />
  </div>
</template>

<script>
import ArchivesSearchForm from './ArchivesSearchForm'
import ArchivesUtils from './ArchivesUtils'
import Qs from 'qs'
import {
  ARCHIVES_TYPES,
  LIMIT_TYPES
} from '@/common/constant/types'

export default {
  name: 'Archives',
  components: {
    ArchivesSearchForm: ArchivesSearchForm
  },
  data () {
    return {
      archives: [],
      ARCHIVES_TYPES: ARCHIVES_TYPES,
      LIMIT_TYPES: LIMIT_TYPES,
      currentPage: 1,
      viewPageCount: 1,
      totalItems: 0,
      limit: LIMIT_TYPES[0],
      searchData: {},
      sort: undefined,
      asc: true,
      orderState: { Category: null, BoardName: null, Title: null, Type: null, Count: null, DateTime: null }
    }
  },
  mounted () {
    if (!this.searchData) {
      this.getArchives(this.searchData)
    }
  },
  methods: {
    momentTime (date) {
      return ArchivesUtils.momentTime(date)
    },
    abbreviation (number) {
      return ArchivesUtils.abbreviation(number)
    },
    tdColoring (archive) {
      return ArchivesUtils.tdColoring(archive)
    },
    substr (str) {
      return ArchivesUtils.substr(str)
    },
    getArchives (searchData) {
      this.searchData = searchData

      var vm = this
      this.$http.get(`${process.env.VUE_APP_URL_BACKEND}/Crawling`, {
        params: {
          offset: this.limit * (this.currentPage - 1),
          limit: this.limit,
          sort: this.sort,
          asc: this.asc
        },
        paramsSerializer (params) {
          return Qs.stringify($.extend(params, searchData), {
            skipNulls: true,
            allowDots: true,
            encode: false,
            arrayFormat: 'repeat'
          })
        } })
        .then((result) => {
          console.log(result)
          this.viewPageCount = Math.ceil(result.data.total / this.limit)
          this.totalItems = result.data.total

          vm.archives = result.data.crawlingDatas
        })
    },
    parentSearching (searchData) {
      this.getArchives(searchData)
    },
    listing (page) {
      this.currentPage = page
      this.getArchives(this.searchData)
    },
    sortBy (key) {
      this.orderState[key] = this.orderState[key] == null ? false : !this.orderState[key]
      for (var orderKey in this.orderState) {
        if (key !== orderKey) {
          this.orderState[orderKey] = null
        }
      }

      this.sort = key
      this.asc = this.orderState[key]
      this.getArchives(this.searchData)
    },
    toArrow (key) {
      if (this.orderState[key] == null) {
        return 'none'
      }

      return this.orderState[key] > 0 ? 'asc' : 'dsc'
    },
    onClickLink (archive, index) {
      window.open(archive.href, '_blank')
    },
    changeLimit (limit) {
      this.limit = limit
      this.getArchives(this.searchData)
    }
  }
}
</script>

<style scoped>

.badge.block-badge {
  display: block;
}

.arrow {
    display: inline-block;
    vertical-align: middle;
    width: 0;
    height: 0;
    margin-left: 5px;
    opacity: 0.66;
}

.arrow.asc {
    display: inline-block;
    border-left: 3px solid transparent;
    border-right: 3px solid transparent;
    border-bottom: 3px solid #FFFFFF;
}

.arrow.dsc {
    display: inline-block;
    border-left: 3px solid transparent;
    border-right: 3px solid transparent;
    border-top: 3px solid #FFFFFF;
}

.badge {
  font-size: 1em !important;
  font-family: Arial !important;
}
.header {
  font-size: 1em !important;
  font-family: Arial !important;
}
.count {
  font-size: 1em !important;
  font-family: Arial !important;
}
.time {
  font-size: 1em !important;
  font-family: Arial !important;
}
.title {
  font-size: 1em !important;
  font-family: Arial !important;
}

.cursor-pointer.unread {
  background: #FFFFFF;
}

.cursor-pointer.read {
  background: #EEEEEE;
}

</style>
