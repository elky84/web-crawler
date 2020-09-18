<template>
  <div id="app container-fluid" align="left" height="100%">
    <h2><i class="fas fa-language"></i><span class="head-text">커뮤니티 보드</span></h2>
    <router-view/>
  </div>
</template>

<script>
export default {
  name: 'app'
}
</script>

<style>
.head-text {
  font-size: 30px !important;
  font-weight: bold;
}
</style>
