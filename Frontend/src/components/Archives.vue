<template>
  <div class="container-fluid">
    <archives-searchForm @searching="parentSearching(... arguments)" ref="searchForm"/>

    <table class="table table-bordered">
      <thead class="thead-dark">
        <tr class="text-center" v-if="!isMobile">
          <th width="120" v-on:click="sortBy('Type')">
            <span class="header">사이트</span>
            <span class="arrow" :class="toArrow('Type')"/>
          </th>
          <th width="120" v-on:click="sortBy('BoardName')">
            <span class="header">게시판</span>
            <span class="arrow" :class="toArrow('BoardName')"/>
          </th>
          <th width="120" v-on:click="sortBy('Category')">
            <span class="header">카테고리</span>
            <span class="arrow" :class="toArrow('Category')"/>
          </th>
          <th v-on:click="sortBy('Title')">
            <span class="header">제목</span>
            <span class="arrow" :class="toArrow('Title')"/>
          </th>
          <th width="120" v-on:click="sortBy('DateTime')">
            <span class="header">시간</span>
            <span class="arrow" :class="toArrow('DateTime')"/>
          </th>
          <th width="120" v-on:click="sortBy('Count')">
            <span class="header">읽음</span>
            <span class="arrow" :class="toArrow('Count')"/>
          </th>
        </tr>
        <tr v-else>
          <th class="th-mobile">
            <span class="header">게시글</span>
          </th>
        </tr>
      </thead>
      <tbody>
        <template v-for="(archive, index) in archives">
          <tr class="cursor-pointer" :key="archive._id" @click.prevent="onClickLink(archive, index)" v-if="isMobile">
            <td class="td-mobile">
              <div class="title row">
                <span class="title">
                  <a v-bind:href=archive.href>
                    {{archive.title}}
                  </a>
                </span>
              </div>
                <span style="display:inline-block" width="120" class="badge block-badge" v-bind:class=getLabel(archive.type)>
                  {{archive.type}}
                </span>

                <span style="display:inline-block" width="120" class="badge block-badge" v-bind:class=getLabel(archive.type)>
                  {{archive.boardName}}
                </span>

                <span style="display:inline-block" width="120" class="badge block-badge" v-bind:class=getLabel(archive.type)
                  v-if="archive.category">
                  {{archive.category}}
                </span>

                <span class="time">
                  {{momentTime(archive.dateTime)}}
                </span>

                <span class="count">
                  {{abbreviation(archive.count)}}
                </span>
            </td>
          </tr>
          <tr class="cursor-pointer" :key="archive._id" @click.prevent="onClickLink(archive, index)" v-if="!isMobile">
            <td align="center">
              <span class="type badge block-badge" v-bind:class=getLabel(archive.type)>
                {{archive.type}}
              </span>
            </td>

            <td align="center">
              <span class="badge block-badge" v-bind:class=getLabel(archive.type)>
                {{archive.boardName}}
              </span>
            </td>

            <td align="center">
              <span v-if="!isEmptyString(archive.category)" class="category badge block-badge" v-bind:class=getLabel(archive.type)>
                {{archive.category}}
              </span>
            </td>

            <td align="left">
              <span class="title">
                <a v-bind:href=archive.href>
                  {{archive.title}}
                </a>
              </span>
            </td>

            <td align="center">
              <span class="time">
                {{momentTime(archive.dateTime)}}
              </span>
            </td>

            <td align="center">
              <span class="count">
                {{abbreviation(archive.count)}}
              </span>
            </td>
          </tr>
        </template>
      </tbody>
    </table>

    <b-pagination align="center" size="md" v-model="currentPage" :total-rows="totalItems" :per-page="searchData.limit" @change="listing(... arguments)" />
  </div>
</template>

<script>
import ArchivesSearchForm from './ArchivesSearchForm'
import ArchivesUtils from './ArchivesUtils'
import Qs from 'qs'
import {
  LABELS,
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
      LABELS: LABELS,
      LIMIT_TYPES: LIMIT_TYPES,
      currentPage: 1,
      totalItems: 0,
      searchData: { limit: LIMIT_TYPES[0] },
      sort: 'DateTime',
      asc: false,
      orderState: { Category: null, BoardName: null, Title: null, Type: null, Count: null, DateTime: -1 }
    }
  },
  mounted () {
    if (!this.searchData) {
      this.getArchives(this.searchData)
    }
  },
  computed: {
    isMobile () {
      if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
        return true
      } else {
        return false
      }
    }
  },
  methods: {
    getLabel (type) {
      return LABELS[type.charCodeAt(0) % LABELS.length]
    },
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
    isEmptyString (str) {
      if (str) {
        return this.trim(str) === ''
      }
      return true
    },
    trim (str) {
      return str.replace(/^\s+|\s+$/g, '')
    },
    getArchives (searchData) {
      this.searchData = Object.assign({}, searchData)

      var vm = this
      this.$http.get(`${process.env.VUE_APP_URL_BACKEND}/Crawling`, {
        params: {
          offset: this.searchData.limit * (this.currentPage - 1),
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
          this.totalItems = result.data.total
          vm.archives = result.data.datas
        })
    },
    parentSearching (searchData) {
      if (JSON.stringify(this.searchData) !== JSON.stringify(searchData)) {
        this.currentPage = 1
      }

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
    border-left: 5px solid transparent;
    border-right: 5px solid transparent;
    border-bottom: 5px solid #FFFFFF;
    border-bottom-color: #0089ff;
}

.arrow.dsc {
    display: inline-block;
    border-left: 5px solid transparent;
    border-right: 5px solid transparent;
    border-top: 5px solid #FFFFFF;
    border-top-color: #0089ff;
}

.badge {
  font-size: 1em !important;
  font-family: Arial !important;
  margin-right: 2px;
  margin-left: 2px;
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
  margin-left: 2px;
  margin-right: 2px;
}

.cursor-pointer.unread {
  background: #FFFFFF;
}

.cursor-pointer.read {
  background: #EEEEEE;
}

th.site-mobile {
  font-size: 11px;
}

th.board-mobile {
  font-size: 11px;
}

th.category-mobile {
  font-size: 11px;
}

th.title-mobile {
  font-size: 11px;
}

th.time-mobile {
  font-size: 11px;
}

th.read-mobile {
  font-size: 9px;
}

td.site-mobile {
  font-size: 9px;
}

td.board-mobile {
  font-size: 9px;
}

td.category-mobile {
  font-size: 9px;
}

td.title-mobile {
  font-size: 11px;
}

td.time-mobile {
  font-size: 10px;
}

td.read-mobile {
  font-size: 9px;
}

th.th-mobile {
  font-size: 10px;
}

td.td-mobile {
  font-size: 10px;
}

</style>
