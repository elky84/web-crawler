import Vue from 'vue'
import Router from 'vue-router'
import Archives from '@/components/Archives'

Vue.use(Router)

export default new Router({
  routes: [
    {
      path: '/',
      name: 'Archives',
      component: Archives
    }
  ]
})
