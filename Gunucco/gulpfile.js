/// <binding ProjectOpened='watch' />
var gulp = require('gulp');
var less = require('gulp-less');

// LESS をコンパイルする
gulp.task('less', () => {
    gulp.src(['./WebFiles/Css/*.less', '!./WebFiles/Css/_*.less'])
        .pipe(less())
        .pipe(gulp.dest('./wwwroot/css'));
});

// watch
gulp.task('watch', () => {
    gulp.watch('./WebFiles/Css/*.less', () => {
        gulp.run('less');
    });
});

// ビルド
gulp.task('build', ['less']);